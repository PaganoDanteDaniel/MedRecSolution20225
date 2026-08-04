using MedRec.DynamicTemplates.ViewModels.Models;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration.Interfaces;

public interface IGetDynamicFieldsOrchestrator
{
    Task<(bool Success, List<DynamicFieldValueModel>? Fields, string? ErrorMessage, bool NotFound)>
        ExecuteAsync(Guid visitId, CancellationToken cts = default);
}