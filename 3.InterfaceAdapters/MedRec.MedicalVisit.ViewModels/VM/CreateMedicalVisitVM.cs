using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.VM;

public class CreateMedicalVisitVM(ICreateMedicalVisitOrchestrator orchestrator)
{
    public CreateMedicalVisitModel Model { get; set; } = new();
    public string InformationMessage { get; set; }


    public event Action OnFinnishOperation;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;

    public async Task LoadDataPatient(Guid patientId, CancellationToken cts = default)
    {
        InformationMessage = "";
        try
        {
            var result = await orchestrator.GetPatient(patientId, cts);

            if (!result.Success)
                RaiseFromError(result.Error);

            Model = result.Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al obtener los datos del paciente", ex);
        }
    }
    public async Task InitializeNewVisit(Guid patientId, CancellationToken cts = default)
    {
        InformationMessage = "";
        try
        {
            var result = await orchestrator.GetHistoryId(patientId, cts);
            if (!result.Success)
                RaiseFromError(result.Error);
            Model.PatientId = patientId;
            Model.MedicalHistoryId = result.Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al obtener la historia clínica del paciente", ex);
        }
    }

    public async Task GetTemplateFields(Guid specialtyId, CancellationToken cts = default)
    {
        InformationMessage = "";
        try
        {
            var result = await orchestrator.GetTemplateFields(specialtyId, cts);
            if (!result.Success)
                RaiseFromError(result.Error);
            Model.TemplateFieldDefinitionModels = result.Value;
        }
        catch (Exception ex)
        {

            throw new InvalidOperationException("Error crítico al obtener los datos de capos dinámicos", ex);
        }
    }
    public async Task AddMedicalVisitAsync(CancellationToken ct)
    {
        InformationMessage = "";
        try
        {
            var result = await orchestrator.CreateMedicalVisit(Model, ct);
            if (!result.Success)
            {
                HandleVisitFailure(result);
                return;
            }
            InformationMessage = "Visita médica registrada exitosamente.";
            OnShowMessage?.Invoke();
            OnFinnishOperation?.Invoke();

        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al registrar la visita", ex);
        }
    }

    private void HandleVisitFailure(OperationResult<Guid> result)
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
                OnShowConcurrencyError?.Invoke();
                break;
            case ErrorCode.DatabaseError:
                OnShowError?.Invoke();
                break;
            default:
                OnShowError?.Invoke();
                break;
        }
    }
}
