using MedRec.BusinessObjects.Results;
using MedRec.MedicalAppointments.ViewModels.Models;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;
public interface IReassignAppointmentAction
{
    Task<OperationResult<Appointment>> ExecuteAsync(Appointment appointment, CancellationToken ct = default);
}