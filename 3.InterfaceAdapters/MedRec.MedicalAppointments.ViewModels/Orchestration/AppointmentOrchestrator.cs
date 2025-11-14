using MedRec.BusinessObjects.Results;
using MedRec.MedicalAppointments.ViewModels.Models;
using MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration;
internal sealed class AppointmentOrchestrator(
    IGetAppointmentsAction getAction,
    ICreateAppointmentAction createAction,
    IMoveAppointmentAction moveAction,
    IReassignAppointmentAction reassignAction,
    IDeleteAppointmentAction deleteAction) : IAppointmentOrchestrator
{
    public Task<OperationResult<IReadOnlyList<Appointment>>> GetByRangeAsync(DateTime start, DateTime end, CancellationToken ct = default) =>
        getAction.ExecuteAsync(start, end, ct);

    public Task<OperationResult<Appointment>> CreateAsync(Appointment appointment, CancellationToken ct = default) =>
        createAction.ExecuteAsync(appointment, ct);

    public Task<OperationResult<Appointment>> MoveAsync(Appointment appointment, CancellationToken ct = default) =>
        moveAction.ExecuteAsync(appointment, ct);

    public Task<OperationResult<Appointment>> ReassignAsync(Appointment appointment, CancellationToken ct = default) =>
        reassignAction.ExecuteAsync(appointment, ct);

    public Task<OperationResult<bool>> DeleteAsync(Guid id, CancellationToken ct = default) =>
        deleteAction.ExecuteAsync(id, ct);
}