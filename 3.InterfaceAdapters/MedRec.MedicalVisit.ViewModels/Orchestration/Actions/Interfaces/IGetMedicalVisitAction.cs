using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;

public interface IGetMedicalVisitAction
{
    Task<OperationResult<GetMedicalVisitDto>> ExecuteAsync(Guid visitId, CancellationToken ct = default);
}
