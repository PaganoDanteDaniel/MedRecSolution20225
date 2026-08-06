using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Shared.Exceptions;

namespace MedRec.Identity.UseCases.Implementations;

public class AuthorizationService(IUserQueriesRepository userQueriesRepository) : IAuthorizationService
{
    public async Task EnsurePermissionAsync(Guid? userId, string permissionCode, CancellationToken ct = default)
    {
        if (userId is null)
            throw new BusinessException(new ErrorInfo("No hay una sesión activa.", ErrorCode.Forbidden, null, 403));

        var permissions = await userQueriesRepository.GetPermissionCodesAsync(userId.Value, ct);
        if (!permissions.Contains(permissionCode))
            throw new BusinessException(new ErrorInfo(
                "No tiene permiso para realizar esta acción.", ErrorCode.Forbidden, new { permissionCode }, 403));
    }
}
