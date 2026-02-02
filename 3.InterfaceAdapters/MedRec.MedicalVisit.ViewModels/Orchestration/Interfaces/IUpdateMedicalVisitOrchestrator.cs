using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.ViewModels.Models;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Interfaces;
public interface IUpdateMedicalVisitOrchestrator
{
    Task<OperationResult<UpdateMedicalVisitModel>> GetPatient(Guid id, CancellationToken ct);
    Task<OperationResult<UpdateMedicalVisitModel>> GetMedicalVisit(Guid id, CancellationToken ct);
    Task<OperationResult<bool>> UpdateMedicalVisit(UpdateMedicalVisitModel model, CancellationToken ct);
}
