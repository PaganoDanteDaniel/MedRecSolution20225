using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class UpdateProfessionalInteractorTests
{
    private static (
        Mock<IUpdateProfessionalOutputPort> presenter,
        Mock<IProfessionalRepositoryUoW> repo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<UpdateProfessionalDto>> validator) CreateMocks()
    {
        return (
            new Mock<IUpdateProfessionalOutputPort>(),
            new Mock<IProfessionalRepositoryUoW>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<UpdateProfessionalDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<UpdateProfessionalDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<UpdateProfessionalDto>(), It.IsAny<Func<UpdateProfessionalDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenProfessionalNotFound()
    {
        var dto = new UpdateProfessionalDto(Guid.NewGuid(), "Ana", "García", "1140001111", ProfessionalType.Receptionist, null, null, Array.Empty<byte>());
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        repo.Setup(r => r.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Professional?)null);

        var interactor = new UpdateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        repo.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateProfessional_WhenValid()
    {
        var existing = new Professional { Id = Guid.NewGuid(), FirstName = "Vieja", LastName = "García", Email = "ana@medrec.local", Type = ProfessionalType.Receptionist };
        var dto = new UpdateProfessionalDto(existing.Id, "Ana", "García", "1140001111", ProfessionalType.Receptionist, null, null, new byte[] { 1 });
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        repo.Setup(r => r.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var interactor = new UpdateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        repo.Verify(r => r.UpdateAsync(It.Is<Professional>(p => p.FirstName == "Ana"), It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
