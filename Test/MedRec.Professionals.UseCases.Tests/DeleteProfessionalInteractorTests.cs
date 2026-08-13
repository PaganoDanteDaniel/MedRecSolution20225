using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class DeleteProfessionalInteractorTests
{
    private static (
        Mock<IDeleteProfessionalOutputPort> presenter,
        Mock<IProfessionalRepositoryUoW> repo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork) CreateMocks()
    {
        return (
            new Mock<IDeleteProfessionalOutputPort>(),
            new Mock<IProfessionalRepositoryUoW>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>());
    }

    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenProfessionalNotFound()
    {
        var id = Guid.NewGuid();
        var (presenter, repo, authorization, currentUser, unitOfWork) = CreateMocks();
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Professional?)null);

        var interactor = new DeleteProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object);

        await interactor.HandleAsync(id, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        repo.Verify(r => r.SoftDeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldSoftDelete_WhenProfessionalExists()
    {
        var id = Guid.NewGuid();
        var (presenter, repo, authorization, currentUser, unitOfWork) = CreateMocks();
        SetUpTransactionToRunWork(unitOfWork);
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Professional { Id = id });

        var interactor = new DeleteProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object);

        await interactor.HandleAsync(id, CancellationToken.None);

        repo.Verify(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
