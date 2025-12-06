using MedRec.BusinessObjects.Results;
using MedRec.Entity.Enums;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.ViewModels.Models;

namespace MedRec.HealthInsurance.ViewModels.VM;
public class CreateHealthInsuranceVM(
    ICreateHealthInsuranceInputPort createInputPort,
    ICreateHealthInsuranceOutputPort createPresenter)
{

    private string _informationMessage;

    #region Events
    public event Action OnHealthInsuranceAdded;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;
    #endregion

    #region Properties
    public CreateHealthInsuranceModel Model { get; set; } = new();
    public string InformationMessage
    {
        get => _informationMessage;
        set
        {
            if (_informationMessage != value)
            {
                _informationMessage = value;
            }
        }
    }
    #endregion

    #region Method
    public async Task AddHealthCompany(CancellationToken ct = default)
    {
        InformationMessage = "";
        ct.ThrowIfCancellationRequested();
        try
        {
            var insuranceHealthCompany = (CreateHealthInsuranceDto)Model;
            await createInputPort.Handle(insuranceHealthCompany, ct);
            var result = createPresenter.Result;
            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join("<br />", result.ValidationErrors.Select(e => e.ErrorMessage));
                OnShowMessage?.Invoke();
            }
            else if (!result.Success)
            {
                HandleErrors(result);
            }
            else
            {
                InformationMessage = "Obra Social registrada exitosamente";
                Model = new CreateHealthInsuranceModel();
                OnHealthInsuranceAdded?.Invoke();
            }

        }
        catch (Exception)
        {
            // Solo para fallos catastróficos (ej: proxy no atrapó la excepción)
            //Logger?.LogError(ex, "Excepción no manejada en AddPatientAsync");
            InformationMessage = "Error inesperado al crear la Obra Social.";
            OnShowError?.Invoke();

            // Opcional: no relanzar si manejo todo en UI
            // Si usas ErrorBoundary y quieres que lo capture, des comentar:
            // throw new InvalidOperationException("Error crítico al crear paciente.", ex);
        }
    }

    private void HandleErrors(OperationResult<bool> result)
    {
        InformationMessage = result.Error?.Message ?? "Error desconocido.";

        switch (result.MessageAction)
        {
            case UserMessageAction.ShowWarning:
                OnShowWarning?.Invoke();
                break;
            case UserMessageAction.ShowConcurrencyMessage:
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
    #endregion
}
