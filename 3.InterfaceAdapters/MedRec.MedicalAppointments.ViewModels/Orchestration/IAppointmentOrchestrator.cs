using MedRec.BusinessObjects.Results;
using MedRec.MedicalAppointments.ViewModels.Models;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration;
public interface IAppointmentOrchestrator
{
    Task<OperationResult<IReadOnlyList<Appointment>>> GetByRangeAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<OperationResult<Appointment>> CreateAsync(Appointment appointment, CancellationToken ct = default);
    Task<OperationResult<Appointment>> MoveAsync(Appointment appointment, CancellationToken ct = default);
    Task<OperationResult<Appointment>> ReassignAsync(Appointment appointment, CancellationToken ct = default);
    Task<OperationResult<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
}