using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalVisit.ViewModels.VM;
public class CreateMedicalVisitVM(
    ICreateMedicalVisitInputPort createMedicalVisitInputPort,
    ICreateMedicalVisitOutputPort createMedicalVisitOutputPort,
    IGetMedicalHistoryIdInputPort getMedicalHistoryIdInputPort,
    IGetMedicalHistoryIdOutputPort getMedicalHistoryIdOutputPort,
    IGetMedicalVisitInputPort getMedicalVisitInputPort,
    IGetMedicalVisitOutputPort getMedicalVisitOutputPort,
    IPatientForMedicalVisitInputPort patientForMedicalVisitInputPort,
    IPatientForMedicalVisitOutputPort patientForMedicalVisitOutputPort)
{

    public event Action OnMedicalVisitAdded;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;


    public CreateMedicalVisitModel Model { get; set; } = new();
    public string InformationMessage { get; set; }


    public async Task LoadDataPatient(Guid patientId, CancellationToken cts = default)
    {
        try
        {
            cts.ThrowIfCancellationRequested();
            await patientForMedicalVisitInputPort.Handle(patientId, cts);

            if (patientForMedicalVisitOutputPort.ErrorMessage is not null)
            {
                HandleErrors(patientForMedicalVisitOutputPort.ErrorMessage);
            }
            else
            {
                var response = patientForMedicalVisitOutputPort.DataPatient;
                Model.FullName = response.FullName;
                Model.DateOfBirth = response.DateOfBirth;
                Model.HealthInsuranceName = response.HealthInsuranceName;
                Model.Acronym = response.Acronym;
                Model.HealthInsuranceCard = response.HealthInsuranceCard;
                Model.HealthInsuranceMemberNumber = response.HealthInsuranceMemberNumber;
                Model.HealthInsurancePlan = response.HealthInsurancePlan;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al obtener la historia clínica del paciente", ex);
        }
    }
    //LoadVisitAsync InitializeNewVisit
    public async Task InitializeNewVisit(Guid patientId, CancellationToken cts = default)
    {
        try
        {
            InformationMessage = "";

            cts.ThrowIfCancellationRequested();

            //await LoadDataPatient(Model.PatientId, cts);

            await getMedicalHistoryIdInputPort.Handle(patientId, cts);

            if (getMedicalHistoryIdOutputPort.ErrorMessage is not null)
            {
                HandleErrors(getMedicalHistoryIdOutputPort.ErrorMessage);
            }
            else
            {
                Model.MedicalHistoryId = getMedicalHistoryIdOutputPort.HistoryId;

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

            await createMedicalVisitInputPort.Handle((CreateMedicalVisitDto)Model, cts);

            if (createMedicalVisitOutputPort.ValidationErrors?.Any() == true)
            {
                InformationMessage = string.Join("<br />", createMedicalVisitOutputPort.ValidationErrors.Select(e => e.ErrorMessage));
                OnShowMessage?.Invoke();
            }
            else if (createMedicalVisitOutputPort.ErrorMessage is not null)
            {
                HandleErrors(createMedicalVisitOutputPort.ErrorMessage);
            }
            else
            {
                InformationMessage = "";
                OnShowMessage?.Invoke();
                OnMedicalVisitAdded?.Invoke();
            }
        }
        catch (Exception ex)
        {

            throw new InvalidOperationException("Error crítico al agregar paciente", ex);
        }
    }
    private void HandleErrors(ErrorInfo error)
    {
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
}
