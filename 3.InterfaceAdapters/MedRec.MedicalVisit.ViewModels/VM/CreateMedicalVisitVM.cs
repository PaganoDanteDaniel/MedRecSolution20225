using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace MedRec.MedicalVisit.ViewModels.VM;
public class CreateMedicalVisitVM(
    ICreateMedicalVisitInputPort createMedicalVisitInputPort,
    ICreateMedicalVisitOutputPort createMedicalVisitOutputPort,
    IGetMedicalHistoryIdInputPort getMedicalHistoryIdInputPort,
    IGetMedicalHistoryIdOutputPort getMedicalHistoryIdOutputPort,
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
        catch (LostConnectionException lce)
        {
            await createMedicalVisitOutputPort.ErrorAsync(new ErrorInfo(
                lce.Message,
                ErrorCode.DatabaseError,
                503));
        }
        catch (ConcurrencyException cx)
        {
            // 409: incluir conflictos tipados (lista de ConcurrencyConflictDto)
            await createMedicalVisitOutputPort.ErrorAsync(new ErrorInfo(
                "Conflicto de concurrencia al crear el turno.",
                ErrorCode.ConcurrencyError,
                cx.Conflicts,
                409));
        }
        catch (DuplicateKeyException dx)
        {
            // 409: conflicto por clave duplicada (Details suele contener entidades implicadas)
            await createMedicalVisitOutputPort.ErrorAsync(new ErrorInfo(
                "Ya existe un registro que viola una restricción de unicidad.",
                ErrorCode.DuplicateKey,
                dx.Details,
                409));
        }
        catch (UpdateException ux)
        {
            // 500: otros errores de persistencia
            await createMedicalVisitOutputPort.ErrorAsync(new ErrorInfo(
                "Error al persistir los cambios en la base de datos.",
                ErrorCode.UpdateError,
                ux.Details,
                500));
        }
        catch (BusinessException bx)
        {
            // Mantener compatibilidad con BusinessException si aparece desde otras capas
            await createMedicalVisitOutputPort.ErrorAsync(bx.Error);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await createMedicalVisitOutputPort.ErrorAsync(new ErrorInfo(
                "Ocurrió un error inesperado al crear el turno.",
                ErrorCode.Unknown,
                new { Exception = ex.Message },
                500));
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
