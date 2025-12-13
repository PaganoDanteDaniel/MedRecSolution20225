using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.ViewModels.Models;
using MedRec.Shared.DTOs;

namespace MedRec.HealthInsurance.ViewModels.VM;
public class UpdateHealthInsuranceVM(
    IUpdateHealthInsuranceInputPort inputPort,
    IUpdateHealthInsuranceOutputPort outputPort,
    IGetHealthInsuranceByIdInputPort getInputPort,
    IGetHealthInsuranceByIdOutputPort getOutputPort)
{
    public UpdateHealthInsuranceModel Model { get; set; } = new();
    public string InformationMessage { get; set; }

    public event Action OnUpdateSuccess;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;

    public async Task GetHealthInsuranceAsync(Guid id, CancellationToken ct)
    {
        await getInputPort.Handle(id, ct);
        var result = getOutputPort.Result;

        if (!result.Success)
        {
            HandleErrors(result.Error, result.MessageAction);
        }
        else
        {
            var healthInsurance = result.Value;
            Model.Id = healthInsurance.Id;
            Model.Name = healthInsurance.Name;
            Model.Acronym = healthInsurance.Acronym;
            Model.RowVersion = healthInsurance.RowVersion;
        }
    }
    public async Task UpdateHealthCompany(CancellationToken ct)
    {
        InformationMessage = "";
        try
        {
            var insuranceHealthCompany = (UpdateHealthInsuranceDto)Model;
            await inputPort.Handle(insuranceHealthCompany);
            var result = outputPort.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join("<br />", result.ValidationErrors.Select(e => e.ErrorMessage));
                OnShowMessage?.Invoke();
            }
            else if (!result.Success)
            {
                HandleErrors(result.Error, result.MessageAction);
            }
            else
            {
                InformationMessage = "Obra Social actualizada exitosamente";
                Model = new UpdateHealthInsuranceModel();
                OnUpdateSuccess?.Invoke();
            }
        }
        catch (Exception ex)
        {
            InformationMessage = ex.Message;
        }
    }
    private void HandleErrors(ErrorInfo error, UserMessageAction action)
    {
        InformationMessage = error?.Message ?? "Error desconocido.";

        switch (action)
        {
            case UserMessageAction.ShowWarning:
                OnShowWarning?.Invoke();
                break;
            case UserMessageAction.ShowConcurrencyMessage:
                InformationMessage += " VALORES EN LA BASE DE DATOS:<br />";
                if (error.Details is IReadOnlyList<ConcurrencyConflictDto> conflicts)
                {
                    InformationMessage += string.Join("<br />",
                        conflicts
                        .Where(x => x.PropertyName != "RowVersion")
                        .Select(x => $"{x.PropertyName}: {x.DataBaseValue}"));
                    InformationMessage += "<br /> VALORES QUE USTED ENVÍA:<br />";
                    InformationMessage += string.Join("<br />",
                        conflicts
                        .Where(x => x.PropertyName != "RowVersion")
                        .Select(x => $"{x.PropertyName}: {x.UserValue}"));
                }
                OnShowConcurrencyError?.Invoke();
                break;
            case UserMessageAction.ShowError:
                OnShowError?.Invoke();
                break;
            case UserMessageAction.ShowInfoMessage:
                OnShowMessage?.Invoke();
                break;
            default: // None o desconocido
                OnShowMessage?.Invoke();
                break;
        }
    }
}
