using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Professionals.UseCases.Implementations;

public class GetProfessionalByIdInteractor(
    IGetProfessionalByIdOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext) : IGetProfessionalByIdInputPort
{
    public async Task HandleAsync(Guid id, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_View, ct);

        var professional = await professionalRepository.GetByIdAsync(id, ct);
        var dto = professional is null
            ? null
            : new ProfessionalDto(
                professional.Id, professional.FirstName, professional.LastName, professional.Email,
                professional.Phone, professional.HireDate, professional.Type, professional.LicenseNumber,
                professional.SpecialtyId, professional.RowVersion);

        await presenter.Handle(dto, ct);
    }
}
