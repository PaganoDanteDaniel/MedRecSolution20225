using MedRec.Entity.DTOs;
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

public class UpdateUserInteractorTests
{
    private static (
        Mock<IUpdateUserOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<UpdateUserDto>> validator) CreateMocks()
    {
        return (
            new Mock<IUpdateUserOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<UpdateUserDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<UpdateUserDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<UpdateUserDto>(), It.IsAny<Func<UpdateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    /// <summary>
    /// ExecuteInTransactionWithRetry es responsabilidad del UnitOfWork real (retry, begin/commit/rollback);
    /// para testear el interactor basta con que el mock invoque el delegate recibido.
    /// </summary>
    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationErrors_WhenDtoIsInvalid()
    {
        var dto = new UpdateUserDto(Guid.NewGuid(), "", Array.Empty<Guid>(), null);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        validator.Setup(v => v.Validate(dto, It.IsAny<Func<UpdateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);
        validator.SetupGet(v => v.Errors).Returns(new[] { new ValidationError("FullName", "El nombre completo es obligatorio.") });

        var interactor = new UpdateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Once);
        commandsRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenUserNotFound()
    {
        var dto = new UpdateUserDto(Guid.NewGuid(), "Nombre", new[] { Guid.NewGuid() }, null);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var interactor = new UpdateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        commandsRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateUser_WhenValid()
    {
        var roleId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var dto = new UpdateUserDto(Guid.NewGuid(), "Nombre Nuevo", new[] { roleId }, doctorId);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        var existingUser = new User { Id = dto.UserId, Email = "user@medrec.local", FullName = "Nombre Viejo" };
        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

        var interactor = new UpdateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.UpdateAsync(
            It.Is<User>(u => u.Id == dto.UserId && u.FullName == "Nombre Nuevo" && u.DoctorId == doctorId),
            It.Is<IReadOnlyList<Guid>>(ids => ids.Contains(roleId)),
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
