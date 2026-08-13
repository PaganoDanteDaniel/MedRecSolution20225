using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Professionals.UseCases.Implementations;

public class DeleteProfessionalInteractor(
    IDeleteProfessionalOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork) : IDeleteProfessionalInputPort
{
    public async Task HandleAsync(Guid id, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_Delete, ct);

        var professional = await professionalRepository.GetByIdAsync(id, ct);
        if (professional is null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Profesional no encontrado.", ErrorCode.NotFound, null, 404));
            return;
        }

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await professionalRepository.SoftDeleteAsync(id, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
