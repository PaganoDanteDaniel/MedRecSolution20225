using MedRec.Entity.Enums;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.ViewModels.Models;

namespace MedRec.Patients.ViewModels.VM;
public class CreatePatientVM
{
    private readonly ICreatePatientInputPort _interactor;
    private readonly ICreatePatientOutputPort _presenter;
    public CreatePatientVM(ICreatePatientInputPort interactor, ICreatePatientOutputPort presenter)
    {
        _interactor = interactor;
        _presenter = presenter;
    }

    public event Action OnPatientAdded;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;

    public CreatePatientModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; }

    public async Task AddPatientAsync(CancellationToken cts = default)
    {
        IsProcessing = true;

        try
        {
            InformationMessage = "";
            await _interactor.HandleAsync((CreatePatientDto)Model, cts);
            var result = _presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join("<br />", result.ValidationErrors.Select(e => e.ErrorMessage));
                OnShowMessage?.Invoke();
            }
            else if (!result.Success)
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
            else
            {
                InformationMessage = "PACIENTE CREADO EXITOSAMENTE.";
                Model = new CreatePatientModel();
                OnPatientAdded?.Invoke();
            }
        }
        catch (Exception)
        {
            // Solo para fallos catastróficos (ej: proxy no atrapó la excepción)
            //Logger?.LogError(ex, "Excepción no manejada en AddPatientAsync");
            InformationMessage = "Error inesperado al crear el paciente.";
            OnShowError?.Invoke();

            // Opcional: no relanzar si manejo todo en UI
            // Si usas ErrorBoundary y quieres que lo capture, des comentar:
            // throw new InvalidOperationException("Error crítico al crear paciente.", ex);
        }
        finally
        {
            IsProcessing = false;
        }

    }
}
