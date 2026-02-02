using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.ViewModels.Models;
using MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration.Actions;
internal sealed class ReassignAppointmentAction(
    IReassignMedicalAppointmentInputPort inPort,
    IReassignMedicalAppointmentOutputPort outPort) : IReassignAppointmentAction
{
    public async Task<OperationResult<Appointment>> ExecuteAsync(Appointment appointment, CancellationToken ct = default)
    {
        try
        {
            var dto = AppointmentMapper.ToReassignDto(appointment);
            await inPort.Handle(dto, ct);

            if (outPort.ErrorMessage is not null)
                return OperationResult.Fail<Appointment>(outPort.ErrorMessage, outPort.ValidationErrors);

            if (outPort.ReassignedAppointmentDto is null)
                return OperationResult.Unknown<Appointment>();

            var model = AppointmentMapper.ToModel(outPort.ReassignedAppointmentDto);

            appointment.DoctorId = model.DoctorId;
            appointment.RowVersion = model.RowVersion;
            appointment.Reason = model.Reason;
            appointment.IsDeleted = model.IsDeleted;

            return OperationResult.Ok(model);
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<Appointment>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<Appointment>(new ErrorInfo($"Error crítico al reasignar el turno: {ex.Message}"), null);
        }
    }
}