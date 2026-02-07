using MedRec.DynamicTemplates.ViewModels.Models;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration.Interfaces;

public interface ISaveDynamicFieldsOrchestrator
{
    Task<(bool Success, int SavedCount, Dictionary<string, List<string>>? ValidationErrors, string? ErrorMessage)>
        ExecuteAsync(SaveDynamicFieldsModel model, CancellationToken cts = default);
}