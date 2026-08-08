using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.ViewModels.VM;
public class UsersListVM(
    IUsersListInputPort listInteractor,
    IUsersListOutputPort listPresenter,
    IToggleUserActiveInputPort toggleActiveInteractor,
    IToggleUserActiveOutputPort toggleActivePresenter,
    IResetUserPasswordInputPort resetPasswordInteractor,
    IResetUserPasswordOutputPort resetPasswordPresenter)
{
    public IReadOnlyList<UserSummaryDto> Users { get; private set; } = Array.Empty<UserSummaryDto>();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await listInteractor.HandleAsync(ct);
            var result = listPresenter.Result;
            Users = result.Success ? result.Value ?? Array.Empty<UserSummaryDto>() : Array.Empty<UserSummaryDto>();
            if (!result.Success)
                InformationMessage = result.Error?.Message ?? "No se pudo cargar el listado de usuarios.";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public async Task ToggleActiveAsync(Guid userId, bool isActive, CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await toggleActiveInteractor.HandleAsync(new ToggleUserActiveDto(userId, isActive), ct);
            var result = toggleActivePresenter.Result;
            if (!result.Success)
                InformationMessage = result.Error?.Message ?? "No se pudo cambiar el estado del usuario.";
            else
                await LoadAsync(ct);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public async Task ResetPasswordAsync(Guid userId, string temporaryPassword, CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await resetPasswordInteractor.HandleAsync(new ResetUserPasswordDto(userId, temporaryPassword), ct);
            var result = resetPasswordPresenter.Result;
            InformationMessage = result.Success
                ? "Contraseña temporal enviada."
                : (result.Error?.Message ?? "No se pudo resetear la contraseña.");
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
