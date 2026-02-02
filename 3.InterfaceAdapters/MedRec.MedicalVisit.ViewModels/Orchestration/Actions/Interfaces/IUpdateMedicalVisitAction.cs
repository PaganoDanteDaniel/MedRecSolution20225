using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.ViewModels.Models;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
public interface IUpdateMedicalVisitAction
{
    Task<OperationResult<bool>> ExecuteAsync(UpdateMedicalVisitModel model, CancellationToken ct = default);
}
