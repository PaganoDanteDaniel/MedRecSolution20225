using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Services;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class SessionService : ISessionService
{
    public bool IsAuthenticated => CurrentUser is not null;
    public AuthResultDto? CurrentUser { get; private set; }
    public event Action? OnSessionChanged;

    public Task LoginAsync(AuthResultDto result, CancellationToken ct = default)
    {
        CurrentUser = result;
        OnSessionChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task LogoutAsync(CancellationToken ct = default)
    {
        CurrentUser = null;
        OnSessionChanged?.Invoke();
        return Task.CompletedTask;
    }

    public void ClearMustChangePassword()
    {
        if (CurrentUser is null) return;
        CurrentUser = new AuthResultDto(
            CurrentUser.UserId, CurrentUser.Email, CurrentUser.FullName, CurrentUser.ProfessionalId,
            CurrentUser.Roles, CurrentUser.Permissions, CurrentUser.Token, CurrentUser.ExpiresAtUtc,
            mustChangePassword: false);
        OnSessionChanged?.Invoke();
    }
}
