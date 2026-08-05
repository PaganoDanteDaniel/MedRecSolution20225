namespace MedRec.Identity.BusinessObjects.Interfaces.Services;
public interface IAuthorizationService
{
    Task EnsurePermissionAsync(Guid? userId, string permissionCode, CancellationToken ct = default);
}
