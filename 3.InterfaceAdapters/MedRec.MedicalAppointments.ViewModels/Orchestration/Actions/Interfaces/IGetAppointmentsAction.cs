using MedRec.BusinessObjects.Results;
using MedRec.MedicalAppointments.ViewModels.Models;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;
public interface IGetAppointmentsAction
{
    Task<OperationResult<IReadOnlyList<Appointment>>> ExecuteAsync(DateTime start, DateTime end, CancellationToken ct = default);
}