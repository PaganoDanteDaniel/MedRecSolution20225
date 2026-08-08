using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;

namespace MedRec.Identity.UseCases.Implementations;

public class ToggleUserActiveInteractor(
    IToggleUserActiveOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork) : IToggleUserActiveInputPort
{
    public async Task HandleAsync(ToggleUserActiveDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_Edit, ct);

        var user = await userQueriesRepository.GetByIdAsync(dto.UserId, ct);
        if (user is null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Usuario no encontrado.", ErrorCode.NotFound, null, 404));
            return;
        }

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await userCommandsRepository.SetActiveAsync(dto.UserId, dto.IsActive, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
