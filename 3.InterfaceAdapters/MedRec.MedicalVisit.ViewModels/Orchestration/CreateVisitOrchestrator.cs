using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration;
internal class CreateVisitOrchestrator(
    IGetPatientAction getPatientAction,
    IGetMedicalHistoryAction getMedicalHistory,
    ICreateMedicalVisitAction createMedicalVisit) : ICreateVisitOrchestrator
{
    public Task<OperationResult<bool>> CreateMedicalVisit(CreateMedicalVisitModel model, CancellationToken ct = default) =>
       createMedicalVisit.ExecuteAsync(model, ct);

    public Task<OperationResult<Guid>> GetHistoryId(Guid id, CancellationToken ct) =>
        getMedicalHistory.ExecuteAsync(id, ct);
    public Task<OperationResult<CreateMedicalVisitModel>> GetPatient(Guid id, CancellationToken ct) =>
        getPatientAction.ExecuteAsync(id, ct);
}
