using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalVisit.ViewModels.VM;

public class UpdateMedicalVisitVM(
    IGetMedicalVisitInputPort getMedicalVisitInputPort,
    IGetMedicalVisitOutputPort getMedicalVisitOutputPort,
    IPatientForMedicalVisitInputPort patientForMedicalVisitInputPort,
    IPatientForMedicalVisitOutputPort patientForMedicalVisitOutputPort,
    IUpdateMedicalVisitInputPort updateMedicalVisitInputPort,
    IUpdateMedicalVisitOutputPort updateMedicalVisitOutputPort)
{
    //  Eventos
    public event Action? OnMedicalVisitUpdated;
    public event Action? OnShowMessage;
    public event Action? OnShowWarning;
    public event Action? OnShowError;
    public event Action? OnShowConcurrencyError;

    //  Modelo y mensaje
    public UpdateMedicalVisitModel Model { get; set; } = new();
    public string InformationMessage { get; set; } = string.Empty;

    //  MÉTODOS PÚBLICOS 

    public async Task LoadDataPatient(Guid patientId, CancellationToken ct = default)
    {
        await HandleInputPortAsync(
            async token => await patientForMedicalVisitInputPort.Handle(patientId, token),
            patientForMedicalVisitOutputPort.ErrorMessage,
            () =>
            {
                var response = patientForMedicalVisitOutputPort.DataPatient;
                Model.FullName = response.FullName;
                Model.DateOfBirth = response.DateOfBirth;
                Model.HealthInsuranceName = response.HealthInsuranceName;
                Model.Acronym = response.Acronym;
                Model.HealthInsuranceCard = response.HealthInsuranceCard;
                Model.HealthInsuranceMemberNumber = response.HealthInsuranceMemberNumber;
                Model.HealthInsurancePlan = response.HealthInsurancePlan;
            },
            "Error crítico al obtener los datos del paciente.",
            ct
        );
    }

    public async Task LoadVisitAsync(Guid visitId, CancellationToken ct = default)
    {
        await HandleInputPortAsync(
            async token => await getMedicalVisitInputPort.Handle(visitId, token),
            getMedicalVisitOutputPort.ErrorMessage,
            () =>
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
            },
            "Error crítico al obtener los datos de la visita médica.",
            ct
        );
    }

    public async Task UpdateMedicalVisitAsync(CancellationToken ct = default)
    {
        try
        {
            InformationMessage = string.Empty;
            await updateMedicalVisitInputPort.Handle((UpdateMedicalVisitDto)Model, ct);

            if (updateMedicalVisitOutputPort.ValidationErrors?.Any() == true)
            {
                InformationMessage = string.Join("<br />",
                    updateMedicalVisitOutputPort.ValidationErrors.Select(e => e.ErrorMessage));
                OnShowMessage?.Invoke();
            }
            else if (updateMedicalVisitOutputPort.ErrorMessage is not null)
            {
                HandleErrors(updateMedicalVisitOutputPort.ErrorMessage);
            }
            else
            {
                InformationMessage = "Visita actualizada con éxito.";
                OnShowMessage?.Invoke();
                OnMedicalVisitUpdated?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelación normal, no se considera error
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al actualizar la visita médica.", ex);
        }
    }

    // MÉTODOS PRIVADOS

    /// <summary>
    /// Encapsula el patrón común: limpiar mensaje, ejecutar InputPort, manejar error y mapear resultado.
    /// </summary>
    private async Task HandleInputPortAsync(
        Func<CancellationToken, Task> inputAction,
        ErrorInfo? errorInfo,
        Action successAction,
        string criticalErrorMessage,
        CancellationToken ct)
    {
        try
        {
            InformationMessage = string.Empty;
            ct.ThrowIfCancellationRequested();

            await inputAction(ct);

            if (errorInfo is not null)
            {
                HandleErrors(errorInfo);
            }
            else
            {
                successAction();
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelación esperada -> no relanzamos ni notificamos
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(criticalErrorMessage, ex);
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
