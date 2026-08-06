using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.UseCases.Implementations;
using MedRec.Shared.Exceptions;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class AuthorizationServiceTests
{
    [Fact]
    public async Task EnsurePermissionAsync_ShouldThrowForbidden_WhenUserIdIsNull()
    {
        var repoMock = new Mock<IUserQueriesRepository>();
        var service = new AuthorizationService(repoMock.Object);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.EnsurePermissionAsync(null, "patients.view", CancellationToken.None));
    }

    [Fact]
    public async Task EnsurePermissionAsync_ShouldThrowForbidden_WhenPermissionNotGranted()
    {
        var userId = Guid.NewGuid();
        var repoMock = new Mock<IUserQueriesRepository>();
        repoMock.Setup(r => r.GetPermissionCodesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "patients.view" });

        var service = new AuthorizationService(repoMock.Object);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.EnsurePermissionAsync(userId, "patients.delete", CancellationToken.None));
    }

    [Fact]
    public async Task EnsurePermissionAsync_ShouldNotThrow_WhenPermissionGranted()
    {
        var userId = Guid.NewGuid();
        var repoMock = new Mock<IUserQueriesRepository>();
        repoMock.Setup(r => r.GetPermissionCodesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "patients.view" });

        var service = new AuthorizationService(repoMock.Object);

        await service.EnsurePermissionAsync(userId, "patients.view", CancellationToken.None);
        // Sin excepción = éxito.
    }
}
