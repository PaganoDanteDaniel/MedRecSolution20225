using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.ViewModels.Models;

namespace MedRec.Identity.ViewModels.VM;
public class ChangePasswordVM(
    IChangePasswordInputPort interactor,
    IChangePasswordOutputPort presenter)
{
    public ChangePasswordModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;
    public bool Success { get; private set; }

    public async Task ChangeAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        Success = false;
        try
        {
            InformationMessage = string.Empty;

            if (Model.NewPassword != Model.ConfirmNewPassword)
            {
                InformationMessage = "La confirmación no coincide con la nueva contraseña.";
                return;
            }

            await interactor.HandleAsync((ChangePasswordDto)Model, ct);
            var result = presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo cambiar la contraseña.";
            }
            else
            {
                Success = true;
                InformationMessage = "Contraseña actualizada correctamente.";
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
