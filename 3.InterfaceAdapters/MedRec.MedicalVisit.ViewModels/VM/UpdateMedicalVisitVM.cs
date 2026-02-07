using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration;
using MedRec.MedicalVisit.ViewModels.Orchestration.Interfaces;
using MedRec.Shared.DTOs;
using MedRec.Shared.Helpers;

namespace MedRec.MedicalVisit.ViewModels.VM;

public class UpdateMedicalVisitVM(IUpdateMedicalVisitOrchestrator orchestrator)
{
    public event Action OnFinnishOperation;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;

    public UpdateMedicalVisitModel Model { get; set; } = new();
    public string InformationMessage { get; set; } = string.Empty;
    public IReadOnlyList<ConcurrencyConflictDto> ConcurrencyError { get; set; }
    public async Task LoadDataPatient(Guid patientId, CancellationToken ct = default)
    {
        InformationMessage = "";
        try
        {
            var result = await orchestrator.GetPatient(patientId, ct);

            if (!result.Success)
                RaiseFromError(result.Error);

            Model = result.Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al obtener los datos del paciente", ex);
        }
    }
    public async Task LoadVisitAsync(Guid visitId, CancellationToken ct = default)
    {
        InformationMessage = "";
        try
        {
            var result = await orchestrator.GetMedicalVisit(visitId, ct);
            if (!result.Success)
            {
                RaiseFromError(result.Error);
                return;
            }

            var visit = MedicalVisitMapper.ToUpdateModel(result.Value);

            // Si ya tenemos datos del paciente, conservarlos y sólo actualizar los campos de la visita.
            var patient = Model ?? new UpdateMedicalVisitModel();

            // Copiar solo los campos de la visita desde 'visit' a 'patient'
            patient.Id = visit.Id;
            patient.MedicalHistoryId = visit.MedicalHistoryId;
            patient.VisitDate = visit.VisitDate;
            patient.Reason = visit.Reason;
            patient.Diagnosis = visit.Diagnosis;
            patient.Treatment = visit.Treatment;
            patient.SystolicPressure = visit.SystolicPressure;
            patient.DiastolicPressure = visit.DiastolicPressure;
            patient.PulsePerMinute = visit.PulsePerMinute;
            patient.Temperature = visit.Temperature;
            patient.Notes = visit.Notes;
            patient.RowVersion = visit.RowVersion;

            Model = patient;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al obtener los datos del paciente", ex);
        }
    }
    public async Task UpdateMedicalVisitAsync(CancellationToken ct = default)
    {
        InformationMessage = "";
        try
        {
            var result = await orchestrator.UpdateMedicalVisit(Model, ct);
            if (!result.Success)
            {
                if (result.ValidationErrors?.Any() == true)
                {
                    InformationMessage = string.Join("<br />", result.ValidationErrors.Select(e => e.ErrorMessage));
                    OnShowMessage?.Invoke();
                }
                else if (result.Error is not null)
                {
                    RaiseFromError(result.Error);
                }
                return;
            }
            else
            {
                ConcurrencyError = Array.Empty<ConcurrencyConflictDto>();
                //OnShowMessage?.Invoke();
                OnFinnishOperation?.Invoke();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al registrar la visita", ex);
        }
    }
    private void RaiseFromError(ErrorInfo? error)
    {
        if (error is null)
        {
            InformationMessage = "Error desconocido";
            OnShowMessage?.Invoke();
            return;
        }

        InformationMessage = error.Message;

        switch (error.Code)
        {
            case ErrorCode.DuplicateKey:
                OnShowWarning?.Invoke();
                break;
            case ErrorCode.ConcurrencyError:

                if (error.Details != null)
                {
                    var prop = error.Details.GetType().GetProperty("Conflicts");
                    ConcurrencyError = prop?.GetValue(error.Details) as IReadOnlyList<ConcurrencyConflictDto>;
                    if (ConcurrencyError != null)
                        Model.ApplyCurrentValues(ConcurrencyError);
                }
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