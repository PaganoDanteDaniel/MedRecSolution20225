using MedRec.BusinessObjects.Results;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
public interface IGetMedicalHistoryAction
{
    Task<OperationResult<Guid>> ExecuteAsync(Guid patientId, CancellationToken cts = default);
}
