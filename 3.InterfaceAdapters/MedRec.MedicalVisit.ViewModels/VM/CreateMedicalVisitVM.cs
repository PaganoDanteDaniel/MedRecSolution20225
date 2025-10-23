using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Models;

namespace MedRec.MedicalVisit.ViewModels.VM;
public class CreateMedicalVisitVM(
    ICreateMedicalVisitInputPort _createMedicalVisitInputPort,
    ICreateMedicalVisitOutputPort _createMedicalVisitOutputPort,
    IGetMedicalHistoryIdInputPort _getMedicalHistoryIdInputPort,
    IGetMedicalHistoryIdOutputPort _getMedicalHistoryIdOutputPort,
    IGetMedicalVisitInputPort _getMedicalVisitInputPort,
    IGetMedicalVisitOutputPort _getMedicalVisitOutputPort)
{

    public event Action OnMedicalVisitAdded;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;


    public CreateMedicalVisitModel Model { get; set; } = new();
    public string InformationMessage { get; set; }

    //LoadVisitAsync InitializeNewVisit
    public async Task LoadVisitAsync(Guid visitId, CancellationToken cts = default)
    {
        try
        {
            InformationMessage = "";
            cts.ThrowIfCancellationRequested();

            await _getMedicalVisitInputPort.Handle(visitId, cts);
            if (_getMedicalVisitOutputPort.ErrorMessage is not null)
            {
                var error = _getMedicalVisitOutputPort.ErrorMessage;
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
                var response = _getMedicalVisitOutputPort.MedicalVisit;

                Model.MedicalHistoryId = response.MedicalHistoryId;
                Model.VisitDate = response.VisitDate;
                Model.Reason = response.Reason;
                Model.Diagnosis = (response.Diagnosis ?? string.Empty).ToUpperInvariant();
                Model.Treatment = (response.Treatment ?? string.Empty).ToUpperInvariant();
                Model.SystolicPressure = response.SystolicPressure;
                Model.DiastolicPressure = response.DiastolicPressure;
                Model.PulsePerMinute = response.PulsePerMinute;
                Model.Temperature = response.Temperature;
                Model.Notes = (response.Notes ?? string.Empty).ToUpperInvariant();

            }

        }
        catch (Exception ex)
        {

            throw new InvalidOperationException("Error crítico al obtener la historia clínica del paciente", ex);
        }
    }
    public async Task InitializeNewVisit(Guid patientId, CancellationToken cts = default)
    {
        try
        {
            InformationMessage = "";

            cts.ThrowIfCancellationRequested();

            await _getMedicalHistoryIdInputPort.Handle(patientId, cts);

            if (_getMedicalHistoryIdOutputPort.ErrorMessage is not null)
            {
                var error = _getMedicalHistoryIdOutputPort.ErrorMessage;
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
                Model.MedicalHistoryId = _getMedicalHistoryIdOutputPort.HistoryId;

            }

        }
        catch (Exception ex)
        {

            throw new InvalidOperationException("Error crítico al obtener la historia clínica del paciente", ex);
        }
    }

    public async Task AddMedicalVisitAsync(CancellationToken cts)
    {
        try
        {
            InformationMessage = "";

            cts.ThrowIfCancellationRequested();

            await _createMedicalVisitInputPort.Handle((CreateMedicalVisitDto)Model, cts);
            if (_createMedicalVisitOutputPort.ValidationErrors?.Any() == true)
            {
                InformationMessage = string.Join("<br />", _createMedicalVisitOutputPort.ValidationErrors.Select(e => e.ErrorMessage));
                OnShowMessage?.Invoke();
            }
            else if (_createMedicalVisitOutputPort.ErrorMessage is not null)
            {
                var error = _createMedicalVisitOutputPort.ErrorMessage;
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
