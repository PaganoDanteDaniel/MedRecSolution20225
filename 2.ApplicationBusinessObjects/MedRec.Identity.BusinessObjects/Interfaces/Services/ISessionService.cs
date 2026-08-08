using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Services;
public interface ISessionService
{
    bool IsAuthenticated { get; }
    AuthResultDto? CurrentUser { get; }
    event Action? OnSessionChanged;
    Task LoginAsync(AuthResultDto result, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    void ClearMustChangePassword();
}
