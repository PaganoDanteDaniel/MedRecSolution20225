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

            if (_presenter.ValidationErrors?.Any() == true)
            {

                InformationMessage = string.Join("<br />", _presenter.ValidationErrors.Select(e => e.ErrorMessage));
                OnShowMessage?.Invoke();
            }
            else if (_presenter.ErrorMessage is not null)
            {
                var error = _presenter.ErrorMessage;
                InformationMessage = error.Message;

                switch (error.Code)
                {
                    case ErrorCode.DuplicateKey:
                        OnShowWarning?.Invoke();
                        break;
                    case ErrorCode.ConcurrencyError:
                        OnShowConcurrencyError?.Invoke();
                        break;
                    case ErrorCode.DatabaseError:
                        OnShowError?.Invoke();
                        break;
                    default:
                        OnShowMessage?.Invoke();
                        break;
                }

            }
            else
            {
                InformationMessage = "Paciente creado con éxito";
                Model = new CreatePatientModel();
                OnPatientAdded?.Invoke();
            }

        }
        catch (Exception ex)
        {
            // Para ErrorBoundary
            throw new InvalidOperationException("Error crítico al agregar paciente", ex);
        }
        finally
        {
            IsProcessing = false;
        }

    }
}
