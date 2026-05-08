using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;

public interface IGetTemplateFieldsAction
{
    //Task<(bool Success, List<TemplateFieldDefinitionModel>? Fields, string? ErrorMessage, bool NotFound)>
    //    ExecuteAsync(Guid specialtyId, CancellationToken cts = default);
    Task<OperationResult<IEnumerable<TemplateFieldDefinitionDto>>>
        ExecuteAsync(Guid specialtyId, CancellationToken cts = default);
}
