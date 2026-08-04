using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.ViewModels.Models;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Interfaces;

public interface IUpdateMedicalVisitOrchestrator
{
    Task<OperationResult<UpdateMedicalVisitModel>> GetPatient(Guid id, CancellationToken ct);
    Task<OperationResult<GetMedicalVisitDto>> GetMedicalVisit(Guid id, CancellationToken ct);
    Task<OperationResult<bool>> UpdateMedicalVisit(UpdateMedicalVisitModel model, CancellationToken ct);
}
