using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Professionals.UseCases.Implementations;

public class ListProfessionalsInteractor(
    IListProfessionalsOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext) : IListProfessionalsInputPort
{
    public async Task HandleAsync(ProfessionalType? typeFilter, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_View, ct);

        var professionals = await professionalRepository.ListAsync(typeFilter, ct);
        await presenter.Handle(professionals, ct);
    }
}
