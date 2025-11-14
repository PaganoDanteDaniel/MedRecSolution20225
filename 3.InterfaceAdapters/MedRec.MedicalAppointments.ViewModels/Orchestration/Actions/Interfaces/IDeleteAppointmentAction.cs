using MedRec.BusinessObjects.Results;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;
public interface IDeleteAppointmentAction
{
    Task<OperationResult<bool>> ExecuteAsync(Guid id, CancellationToken ct = default);
}