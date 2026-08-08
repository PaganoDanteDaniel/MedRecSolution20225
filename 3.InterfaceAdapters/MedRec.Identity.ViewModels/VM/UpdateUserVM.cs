using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.ViewModels.Models;

namespace MedRec.Identity.ViewModels.VM;
public class UpdateUserVM(
    IUpdateUserInputPort interactor,
    IUpdateUserOutputPort presenter)
{
    public UpdateUserModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;
    public bool Success { get; private set; }

    public async Task UpdateAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        Success = false;
        try
        {
            InformationMessage = string.Empty;
            await interactor.HandleAsync((UpdateUserDto)Model, ct);
            var result = presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo editar el usuario.";
            }
            else
            {
                InformationMessage = "Usuario actualizado correctamente.";
                Success = true;
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
