using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.ViewModels.Models;

namespace MedRec.MedicalVisit.ViewModels.Orchestration;
public interface ICreateVisitOrchestrator
{
    Task<OperationResult<CreateMedicalVisitModel>> GetPatient(Guid id, CancellationToken ct);
    Task<OperationResult<Guid>> GetHistoryId(Guid id, CancellationToken ct);
    Task<OperationResult<bool>> CreateMedicalVisit(CreateMedicalVisitModel model, CancellationToken ct = default);
}
