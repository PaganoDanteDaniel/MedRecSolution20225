using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using MedRec.Shared.Exceptions;
using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class CreateProfessionalInteractorTests
{
    private static (
        Mock<ICreateProfessionalOutputPort> presenter,
        Mock<IProfessionalRepositoryUoW> repo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<CreateProfessionalDto>> validator) CreateMocks()
    {
        return (
            new Mock<ICreateProfessionalOutputPort>(),
            new Mock<IProfessionalRepositoryUoW>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<CreateProfessionalDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<CreateProfessionalDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<CreateProfessionalDto>(), It.IsAny<Func<CreateProfessionalDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCallerLacksPermission()
    {
        var dto = new CreateProfessionalDto("Ana", "García", "ana@medrec.local", null, DateTime.Today, ProfessionalType.Receptionist, null, null);
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        authorization.Setup(a => a.EnsurePermissionAsync(It.IsAny<Guid?>(), "professionals.create", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException(new ErrorInfo("No tiene permiso.", ErrorCode.Forbidden)));

        var interactor = new CreateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await Assert.ThrowsAsync<BusinessException>(() => interactor.HandleAsync(dto, CancellationToken.None));

        repo.Verify(r => r.CreateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationErrors_WhenDoctorHasNoSpecialty()
    {
        var dto = new CreateProfessionalDto("Ana", "García", "ana@medrec.local", null, DateTime.Today, ProfessionalType.Doctor, "MP123", null);
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        validator.Setup(v => v.Validate(dto, It.IsAny<Func<CreateProfessionalDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);
        validator.SetupGet(v => v.Errors).Returns(new[] { new ValidationError("SpecialtyId", "La especialidad es obligatoria para médicos.") });

        var interactor = new CreateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Once);
        repo.Verify(r => r.CreateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenEmailAlreadyExists()
    {
        var dto = new CreateProfessionalDto("Ana", "García", "existente@medrec.local", null, DateTime.Today, ProfessionalType.Receptionist, null, null);
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        repo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Professional { Id = Guid.NewGuid(), Email = dto.Email });

        var interactor = new CreateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        repo.Verify(r => r.CreateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateProfessional_WhenValid()
    {
        var dto = new CreateProfessionalDto("Ana", "García", "ana@medrec.local", "1140001111", DateTime.Today, ProfessionalType.Receptionist, null, null);
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        repo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync((Professional?)null);

        var interactor = new CreateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        repo.Verify(r => r.CreateAsync(
            It.Is<Professional>(p => p.Email == dto.Email && p.Type == ProfessionalType.Receptionist && p.IsDeleted == false),
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
