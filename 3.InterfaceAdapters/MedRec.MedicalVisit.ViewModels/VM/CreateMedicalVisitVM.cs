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
    IPatientForMedicalVisitOutputPort patientForMedicalVisitOutputPort,
    IUpdateMedicalVisitInputPort updateMedicalVisitInputPort,
    IUpdateMedicalVisitOutputPort updateMedicalVisitOutputPort)
{

    public event Action OnMedicalVisitAdded;
    public event Action OnMedicalVisitUpdated;
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
    public async Task LoadVisitAsync(Guid visitId, CancellationToken cts = default)
    {
        try
        {
            InformationMessage = "";
            cts.ThrowIfCancellationRequested();

            //await LoadDataPatient(Model.PatientId, cts);

            await getMedicalVisitInputPort.Handle(visitId, cts);
            if (getMedicalVisitOutputPort.ErrorMessage is not null)
            {
                HandleErrors(getMedicalVisitOutputPort.ErrorMessage);
            }
            else
            {
                var response = getMedicalVisitOutputPort.MedicalVisit;
                Model.Id = response.Id;
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
                Model.RowVersion = response.RowVersion;
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

            await createMedicalVisitInputPort.Handle((MedicalVisitDto)Model, cts);
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
    public async Task UpdateMedicalVisitAsync(CancellationToken cts)
    {
        try
        {
            InformationMessage = "";
            await updateMedicalVisitInputPort.Handle((MedicalVisitDto)Model, cts);
            if (updateMedicalVisitOutputPort.ValidationErrors?.Any() == true)
            {
                InformationMessage = string.Join("<br />", updateMedicalVisitOutputPort.ValidationErrors.Select(e => e.ErrorMessage));
                OnShowMessage?.Invoke();
            }
            else if (updateMedicalVisitOutputPort.ErrorMessage is not null)
            {
                HandleErrors(updateMedicalVisitOutputPort.ErrorMessage);
            }
            else
            {
                InformationMessage = "Visita registrada con exito.";
                //Model = new CreateMedicalVisitModel();
                OnShowMessage?.Invoke();
                OnMedicalVisitUpdated?.Invoke();
            }
        }
        catch (Exception)
        {

            throw;
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
