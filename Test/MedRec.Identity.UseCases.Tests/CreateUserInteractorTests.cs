using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
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

public class CreateUserInteractorTests
{
    private static (
        Mock<ICreateUserOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IPasswordHasher> hasher,
        Mock<IEmailNotificationService> email,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<CreateUserDto>> validator) CreateMocks()
    {
        return (
            new Mock<ICreateUserOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IPasswordHasher>(),
            new Mock<IEmailNotificationService>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<CreateUserDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<CreateUserDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<CreateUserDto>(), It.IsAny<Func<CreateUserDto, IReadOnlyList<ValidationError>>>()))
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
        var dto = new CreateUserDto("nuevo@medrec.local", "Nuevo Usuario", "Temporal123!", new[] { Guid.NewGuid() }, null);
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        authorization.Setup(a => a.EnsurePermissionAsync(It.IsAny<Guid?>(), "users.create", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException(new ErrorInfo("No tiene permiso.", MedRec.Entity.Enums.ErrorCode.Forbidden)));

        var interactor = new CreateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await Assert.ThrowsAsync<BusinessException>(() => interactor.HandleAsync(dto, CancellationToken.None));

        commandsRepo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationErrors_WhenDtoIsInvalid()
    {
        var dto = new CreateUserDto("", "", "", Array.Empty<Guid>(), null);
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        validator.Setup(v => v.Validate(dto, It.IsAny<Func<CreateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);
        validator.SetupGet(v => v.Errors).Returns(new[] { new ValidationError("Email", "El email es obligatorio.") });

        var interactor = new CreateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Once);
        commandsRepo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenEmailAlreadyExists()
    {
        var dto = new CreateUserDto("existente@medrec.local", "Alguien", "Temporal123!", new[] { Guid.NewGuid() }, null);
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        queriesRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = dto.Email });

        var interactor = new CreateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        commandsRepo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateUserAndSendEmail_WhenValid()
    {
        var roleId = Guid.NewGuid();
        var dto = new CreateUserDto("nuevo@medrec.local", "Nuevo Usuario", "Temporal123!", new[] { roleId }, null);
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        queriesRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        hasher.Setup(h => h.Hash(dto.TemporaryPassword)).Returns("hashed");

        var interactor = new CreateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.Email == dto.Email && u.PasswordHash == "hashed" && u.MustChangePassword == true && u.IsActive == true),
            It.Is<IReadOnlyList<Guid>>(ids => ids.Contains(roleId)),
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        email.Verify(e => e.SendTemporaryPasswordAsync(dto.Email, dto.FullName, dto.TemporaryPassword, It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
