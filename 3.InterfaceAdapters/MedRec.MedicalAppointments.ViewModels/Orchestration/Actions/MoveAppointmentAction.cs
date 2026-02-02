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
            var result = outPort.Result;

            if (!result.Success)
                return OperationResult.Fail<Appointment>(result.Error, result.ValidationErrors);

            var model = AppointmentMapper.ToModel(result.Value);

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
            return OperationResult.Fail<Appointment>(new ErrorInfo($"Error crítico al mover el turno: {ex.Message}"), null);
        }
    }
}