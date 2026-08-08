using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;
using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class ChangePasswordInteractorTests
{
    private static (
        Mock<IChangePasswordOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IPasswordHasher> hasher,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<ChangePasswordDto>> validator) CreateMocks()
    {
        return (
            new Mock<IChangePasswordOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IPasswordHasher>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<ChangePasswordDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<ChangePasswordDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<ChangePasswordDto>(), It.IsAny<Func<ChangePasswordDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    /// <summary>
    /// ExecuteInTransactionWithRetry es responsabilidad del UnitOfWork real (retry, begin/commit/rollback);
    /// para testear el interactor basta con que el mock invoque el delegate recibido.
    /// </summary>
    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnInvalidCurrentPassword_WhenCurrentPasswordDoesNotMatch()
    {
        var userId = Guid.NewGuid();
        var dto = new ChangePasswordDto("ClaveVieja", "ClaveNueva123!");
        var (presenter, commandsRepo, queriesRepo, hasher, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        currentUser.SetupGet(c => c.UserId).Returns(userId);
        var user = new User { Id = userId, PasswordHash = "hash-actual" };
        queriesRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.CurrentPassword, user.PasswordHash)).Returns(false);

        var interactor = new ChangePasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.InvalidCurrentPassword(), Times.Once);
        commandsRepo.Verify(r => r.SetPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldChangePasswordAndClearFlag_WhenCurrentPasswordMatches()
    {
        var userId = Guid.NewGuid();
        var dto = new ChangePasswordDto("ClaveVieja", "ClaveNueva123!");
        var (presenter, commandsRepo, queriesRepo, hasher, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        currentUser.SetupGet(c => c.UserId).Returns(userId);
        var user = new User { Id = userId, PasswordHash = "hash-actual" };
        queriesRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.CurrentPassword, user.PasswordHash)).Returns(true);
        hasher.Setup(h => h.Hash(dto.NewPassword)).Returns("hash-nuevo");

        var interactor = new ChangePasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.SetPasswordAsync(userId, "hash-nuevo", false, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.InvalidCurrentPassword(), Times.Never);
    }
}
