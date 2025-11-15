using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.ViewModels.Models;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
public interface IGetMedicalVisitAction
{
    Task<OperationResult<UpdateMedicalVisitModel>> ExecuteAsync(Guid visitId, CancellationToken ct = default);
}
