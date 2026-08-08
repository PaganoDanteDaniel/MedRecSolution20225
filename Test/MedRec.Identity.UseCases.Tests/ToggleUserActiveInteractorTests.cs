using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class ToggleUserActiveInteractorTests
{
    private static (
        Mock<IToggleUserActiveOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork) CreateMocks()
    {
        return (
            new Mock<IToggleUserActiveOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>());
    }

    /// <summary>
    /// ExecuteInTransactionWithRetry es responsabilidad del UnitOfWork real (retry, begin/commit/rollback);
    /// para testear el interactor basta con que el mock invoque el delegate recibido.
    /// </summary>
    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenUserNotFound()
    {
        var dto = new ToggleUserActiveDto(Guid.NewGuid(), false);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork) = CreateMocks();

        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var interactor = new ToggleUserActiveInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        commandsRepo.Verify(r => r.SetActiveAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldToggleActive_WhenUserExists()
    {
        var dto = new ToggleUserActiveDto(Guid.NewGuid(), false);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork) = CreateMocks();
        SetUpTransactionToRunWork(unitOfWork);

        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = dto.UserId, IsActive = true });

        var interactor = new ToggleUserActiveInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.SetActiveAsync(dto.UserId, false, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
