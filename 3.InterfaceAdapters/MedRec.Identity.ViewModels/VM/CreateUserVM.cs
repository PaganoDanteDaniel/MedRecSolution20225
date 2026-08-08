using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.ViewModels.Models;

namespace MedRec.Identity.ViewModels.VM;
public class CreateUserVM(
    ICreateUserInputPort interactor,
    ICreateUserOutputPort presenter)
{
    public CreateUserModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;
    public bool Success { get; private set; }

    public async Task CreateAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        Success = false;
        try
        {
            InformationMessage = string.Empty;
            await interactor.HandleAsync((CreateUserDto)Model, ct);
            var result = presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo crear el usuario.";
            }
            else
            {
                InformationMessage = "Usuario creado correctamente.";
                Success = true;
                Model = new CreateUserModel();
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
