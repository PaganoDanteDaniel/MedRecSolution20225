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

public class AuthenticateUserInteractorTests
{
    private static (
        Mock<IAuthenticateUserOutputPort> presenter,
        Mock<IUserQueriesRepository> userRepo,
        Mock<IPasswordHasher> hasher,
        Mock<IAuthTokenGenerator> tokenGenerator,
        Mock<IModelValidatorHub<AuthenticateUserDto>> validator) CreateMocks()
    {
        return (
            new Mock<IAuthenticateUserOutputPort>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IPasswordHasher>(),
            new Mock<IAuthTokenGenerator>(),
            new Mock<IModelValidatorHub<AuthenticateUserDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<AuthenticateUserDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<AuthenticateUserDto>(), It.IsAny<Func<AuthenticateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationErrors_WhenDtoIsInvalid()
    {
        var dto = new AuthenticateUserDto("", "");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();

        validator.Setup(v => v.Validate(dto, It.IsAny<Func<AuthenticateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);
        validator.SetupGet(v => v.Errors).Returns(new[] { new ValidationError("Email", "El email es obligatorio.") });

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Once);
        presenter.Verify(p => p.InvalidCredentials(), Times.Never);
        presenter.Verify(p => p.Handle(It.IsAny<AuthResultDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnInvalidCredentials_WhenUserNotFound()
    {
        var dto = new AuthenticateUserDto("nadie@medrec.local", "Cambiar123!");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.InvalidCredentials(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnInvalidCredentials_WhenPasswordDoesNotMatch()
    {
        var dto = new AuthenticateUserDto("admin@medrec.local", "MalaClave");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, PasswordHash = "hash", IsActive = true };
        userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(false);

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.InvalidCredentials(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnInvalidCredentials_WhenUserIsInactive()
    {
        var dto = new AuthenticateUserDto("admin@medrec.local", "Cambiar123!");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, PasswordHash = "hash", IsActive = false };
        userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.InvalidCredentials(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAuthResult_WhenCredentialsAreValid()
    {
        var dto = new AuthenticateUserDto("admin@medrec.local", "Cambiar123!");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, PasswordHash = "hash", FullName = "Admin", IsActive = true, ProfessionalId = null, MustChangePassword = true };
        userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);
        userRepo.Setup(r => r.GetRoleNamesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "Administrador" });
        userRepo.Setup(r => r.GetPermissionCodesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "patients.view" });
        tokenGenerator.Setup(t => t.GenerateToken(user.Id, user.Email, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IReadOnlyList<string>>()))
            .Returns(("token123", DateTime.UtcNow.AddHours(4)));

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.Handle(
            It.Is<AuthResultDto>(r => r.UserId == user.Id && r.Token == "token123" && r.Roles.Contains("Administrador") && r.MustChangePassword == true),
            It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.InvalidCredentials(), Times.Never);
    }
}
