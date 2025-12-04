using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.ViewModels.Models;
using MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration.Actions;
internal sealed class GetAppointmentsAction(
    IGetMedicalAppointmentsInputPort inPort,
    IGetMedicalAppointmentsOutputPort outPort) : IGetAppointmentsAction
{
    public async Task<OperationResult<IReadOnlyList<Appointment>>> ExecuteAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        try
        {
            await inPort.Handle((start, end), ct);

            if (outPort.ErrorMessage is not null)
                return OperationResult.Fail<IReadOnlyList<Appointment>>(outPort.ErrorMessage, outPort.ValidationErrors);

            var dtos = outPort.AppointmentsDto ?? Enumerable.Empty<MedRec.MedicalAppointments.BusinessObjects.DTOs.MedicalAppointmentDto>();
            var list = dtos.Select(AppointmentMapper.ToModel).ToList().AsReadOnly();
            return OperationResult.Ok<IReadOnlyList<Appointment>>(list);
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<IReadOnlyList<Appointment>>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<IReadOnlyList<Appointment>>(new ErrorInfo($"Error crítico al obtener los turnos: {ex.Message}"), null);
        }
    }
}