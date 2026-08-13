using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class GetProfessionalByIdInteractorTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnNullDto_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var presenter = new Mock<IGetProfessionalByIdOutputPort>();
        var repo = new Mock<IProfessionalRepositoryUoW>();
        var authorization = new Mock<IAuthorizationService>();
        var currentUser = new Mock<ICurrentUserContext>();
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Professional?)null);

        var interactor = new GetProfessionalByIdInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object);

        await interactor.HandleAsync(id, CancellationToken.None);

        presenter.Verify(p => p.Handle((ProfessionalDto?)null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
