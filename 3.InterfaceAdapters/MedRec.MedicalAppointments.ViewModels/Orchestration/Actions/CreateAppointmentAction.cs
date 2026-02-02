using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.ViewModels.Models;
using MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration.Actions;
internal sealed class CreateAppointmentAction(
    ICreateMedicalAppointmentInputPort inPort,
    ICreateMedicalAppointmentOutputPort outPort) : ICreateAppointmentAction
{
    public async Task<OperationResult<Appointment>> ExecuteAsync(Appointment appointment, CancellationToken ct = default)
    {
        try
        {
            var dto = AppointmentMapper.ToCreateDto(appointment);
            await inPort.Handle(dto, ct);

            var result = outPort.Result;

            if (!result.Success)
                return OperationResult.Fail<Appointment>(result.Error, result.ValidationErrors);

            var model = AppointmentMapper.ToModel(result.Value);

            // Sincronizar RowVersion y demás en la instancia original (opcional)
            appointment.RowVersion = model.RowVersion;
            appointment.Id = model.Id;
            appointment.IsDeleted = model.IsDeleted;

            return OperationResult.Ok(model);
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<Appointment>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<Appointment>(
                new ErrorInfo($"Error crítico al crear el turno: {ex.Message}"), null);
        }
    }
}