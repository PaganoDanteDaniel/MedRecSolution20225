using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.ViewModels.Models;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
public interface ICreateMedicalVisitAction
{
    Task<OperationResult<bool>> ExecuteAsync(CreateMedicalVisitModel model, CancellationToken ct = default);
}
