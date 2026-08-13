using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class ListProfessionalsInteractorTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnListFromRepository()
    {
        var presenter = new Mock<IListProfessionalsOutputPort>();
        var repo = new Mock<IProfessionalRepositoryUoW>();
        var authorization = new Mock<IAuthorizationService>();
        var currentUser = new Mock<ICurrentUserContext>();

        var list = new List<ProfessionalDto>
        {
            new(Guid.NewGuid(), "Ana", "García", "ana@medrec.local", "", DateTime.Today, ProfessionalType.Receptionist, null, null, Array.Empty<byte>())
        };
        repo.Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var interactor = new ListProfessionalsInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object);

        await interactor.HandleAsync(null, CancellationToken.None);

        presenter.Verify(p => p.Handle(list, It.IsAny<CancellationToken>()), Times.Once);
    }
}
