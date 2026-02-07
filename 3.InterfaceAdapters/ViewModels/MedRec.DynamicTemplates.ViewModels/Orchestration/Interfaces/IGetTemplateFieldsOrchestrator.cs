using MedRec.DynamicTemplates.ViewModels.Models;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration.Interfaces;

public interface IGetTemplateFieldsOrchestrator
{
    Task<(bool Success, List<TemplateFieldDefinitionModel>? Fields, string? ErrorMessage, bool NotFound)>
        ExecuteAsync(Guid specialtyId, CancellationToken cts = default);
}