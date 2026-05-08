using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Models;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Interfaces;

public interface ICreateMedicalVisitOrchestrator
{
    Task<OperationResult<CreateMedicalVisitModel>> GetPatient(Guid id, CancellationToken ct);
    Task<OperationResult<Guid>> GetHistoryId(Guid id, CancellationToken ct);
    Task<OperationResult<List<TemplateFieldDefinitionModel>>> GetTemplateFields(Guid specialtyId, CancellationToken cts = default);
    Task<OperationResult<Guid>> CreateMedicalVisit(CreateMedicalVisitModel model, CancellationToken ct = default);
}
