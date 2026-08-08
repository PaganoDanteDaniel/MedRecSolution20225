using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;
using MedRec.Shared.Exceptions;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class UsersListInteractorTests
{
    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCallerLacksPermission()
    {
        var presenter = new Mock<IUsersListOutputPort>();
        var queriesRepo = new Mock<IUserQueriesRepository>();
        var authorization = new Mock<IAuthorizationService>();
        var currentUser = new Mock<ICurrentUserContext>();

        authorization.Setup(a => a.EnsurePermissionAsync(It.IsAny<Guid?>(), "users.view", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException(new MedRec.Entity.DTOs.ErrorInfo("No tiene permiso.", MedRec.Entity.Enums.ErrorCode.Forbidden)));

        var interactor = new UsersListInteractor(presenter.Object, queriesRepo.Object, authorization.Object, currentUser.Object);

        await Assert.ThrowsAsync<BusinessException>(() => interactor.HandleAsync(CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUsers_WhenPermissionGranted()
    {
        var presenter = new Mock<IUsersListOutputPort>();
        var queriesRepo = new Mock<IUserQueriesRepository>();
        var authorization = new Mock<IAuthorizationService>();
        var currentUser = new Mock<ICurrentUserContext>();

        var users = new List<UserSummaryDto>
        {
            new(Guid.NewGuid(), "admin@medrec.local", "Administrador", true, new List<string> { "Administrador" })
        };
        queriesRepo.Setup(r => r.ListWithRolesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        var interactor = new UsersListInteractor(presenter.Object, queriesRepo.Object, authorization.Object, currentUser.Object);

        await interactor.HandleAsync(CancellationToken.None);

        presenter.Verify(p => p.Handle(users, It.IsAny<CancellationToken>()), Times.Once);
    }
}
