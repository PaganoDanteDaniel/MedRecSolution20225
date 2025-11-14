using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.ViewModels.Models;
using MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration.Actions;
internal sealed class MoveAppointmentAction(
    IMoveMedicalAppointmentInputPort inPort,
    IMoveMedicalAppointmentOutputPort outPort) : IMoveAppointmentAction
{
    public async Task<OperationResult<Appointment>> ExecuteAsync(Appointment appointment, CancellationToken ct = default)
    {
        try
        {
            var dto = AppointmentMapper.ToMoveDto(appointment);
            await inPort.Handle(dto, ct);

            if (outPort.ErrorMessage is not null)
                return OperationResult.Fail<Appointment>(outPort.ErrorMessage, outPort.ValidationErrors);

            if (outPort.movedMedicalAppointmentDto is null)
                return OperationResult.Unknown<Appointment>();

            var model = AppointmentMapper.ToModel(outPort.movedMedicalAppointmentDto);

            appointment.DateTime = model.DateTime;
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
            return OperationResult.Fail<Appointment>(new ErrorInfo($"Error crítico al mover el turno: {ex.Message}"));
        }
    }
}