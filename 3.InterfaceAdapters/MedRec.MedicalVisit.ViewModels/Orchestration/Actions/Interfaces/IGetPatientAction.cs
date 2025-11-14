using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.ViewModels.Models;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
public interface IGetPatientAction
{
    Task<OperationResult<CreateMedicalVisitModel>> ExecuteAsync(Guid patientId, CancellationToken cts = default);
}
