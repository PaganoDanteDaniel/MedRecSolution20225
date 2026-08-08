using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;
using MedRec.Shared.Exceptions;
using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class ResetUserPasswordInteractorTests
{
    private static (
        Mock<IResetUserPasswordOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IPasswordHasher> hasher,
        Mock<IEmailNotificationService> email,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<ResetUserPasswordDto>> validator) CreateMocks()
    {
        return (
            new Mock<IResetUserPasswordOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IPasswordHasher>(),
            new Mock<IEmailNotificationService>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<ResetUserPasswordDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<ResetUserPasswordDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<ResetUserPasswordDto>(), It.IsAny<Func<ResetUserPasswordDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    /// <summary>
    /// ExecuteInTransactionWithRetry es responsabilidad del UnitOfWork real (retry, begin/commit/rollback);
    /// para testear el interactor basta con que el mock invoque el delegate recibido.
    /// </summary>
    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCallerLacksPermission()
    {
        var dto = new ResetUserPasswordDto(Guid.NewGuid(), "NuevaTemp123!");
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        authorization.Setup(a => a.EnsurePermissionAsync(It.IsAny<Guid?>(), SystemPermissions.Users_Edit, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException(new ErrorInfo("No tiene permiso.", MedRec.Entity.Enums.ErrorCode.Forbidden)));

        var interactor = new ResetUserPasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await Assert.ThrowsAsync<BusinessException>(() => interactor.HandleAsync(dto, CancellationToken.None));

        commandsRepo.Verify(r => r.SetPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationErrors_WhenDtoIsInvalid()
    {
        var dto = new ResetUserPasswordDto(Guid.NewGuid(), "");
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        validator.Setup(v => v.Validate(dto, It.IsAny<Func<ResetUserPasswordDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);
        validator.SetupGet(v => v.Errors).Returns(new[] { new ValidationError("TemporaryPassword", "La contraseña temporal es obligatoria.") });

        var interactor = new ResetUserPasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Once);
        commandsRepo.Verify(r => r.SetPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenUserNotFound()
    {
        var dto = new ResetUserPasswordDto(Guid.NewGuid(), "NuevaTemp123!");
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var interactor = new ResetUserPasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        commandsRepo.Verify(r => r.SetPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldSetTemporaryPasswordAndSendEmail_WhenUserExists()
    {
        var dto = new ResetUserPasswordDto(Guid.NewGuid(), "NuevaTemp123!");
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        var user = new User { Id = dto.UserId, Email = "user@medrec.local", FullName = "Usuario Existente" };
        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Hash(dto.TemporaryPassword)).Returns("hashed-nuevo");

        var interactor = new ResetUserPasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.SetPasswordAsync(dto.UserId, "hashed-nuevo", true, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        email.Verify(e => e.SendTemporaryPasswordAsync(user.Email, user.FullName, dto.TemporaryPassword, It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
