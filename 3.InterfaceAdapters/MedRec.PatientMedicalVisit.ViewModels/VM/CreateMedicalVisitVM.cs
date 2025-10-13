using MedRec.Entity.Enums;
using MedRec.PatientMedicalVisit.BusinessObjects.DTOs;
using MedRec.PatientMedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.PatientMedicalVisit.ViewModels.Models;

namespace MedRec.PatientMedicalVisit.ViewModels.VM;
public class CreateMedicalVisitVM(
    ICreateMedicalVisitInputPort interactor,
    ICreateMedicalVisitOutputPort presenter)
{
    private readonly ICreateMedicalVisitInputPort _interactor = interactor;
    private readonly ICreateMedicalVisitOutputPort _presenter = presenter;

    public event Action OnMedicalVisitAdded;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;


    public CreateMedicalVisitModel Model { get; set; } = new();
    public string InformationMessage { get; set; }


    public async Task AddMedicalVisitAsync()
    {
        try
        {
            InformationMessage = "";

            await _interactor.Handle((CreateMedicalVisitDto)Model);
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
                InformationMessage = "Visita registrada con exito.";
                Model = new CreateMedicalVisitModel();
                OnShowMessage?.Invoke();
                OnMedicalVisitAdded?.Invoke();
            }
        }
        catch (Exception ex)
        {

            throw new InvalidOperationException("Error crítico al agregar paciente", ex);
        }

    }
}
