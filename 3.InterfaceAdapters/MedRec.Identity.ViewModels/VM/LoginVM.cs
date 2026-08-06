using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.ViewModels.Models;

namespace MedRec.Identity.ViewModels.VM;
public class LoginVM(
    IAuthenticateUserInputPort interactor,
    IAuthenticateUserOutputPort presenter,
    ISessionService sessionService)
{
    public LoginModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;

    public async Task LoginAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            InformationMessage = string.Empty;
            await interactor.HandleAsync((AuthenticateUserDto)Model, ct);
            var result = presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "Email o contraseña incorrectos.";
            }
            else
            {
                await sessionService.LoginAsync(result.Value!, ct);
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
