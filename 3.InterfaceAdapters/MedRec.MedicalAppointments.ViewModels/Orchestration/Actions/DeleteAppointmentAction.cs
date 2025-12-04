using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration.Actions;
internal sealed class DeleteAppointmentAction(
    IDeleteMedicalAppointmentInputPort inPort,
    IDeleteMedicalAppointmentOutputPort outPort) : IDeleteAppointmentAction
{
    public async Task<OperationResult<bool>> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await inPort.Handle(id, ct);

            if (outPort.ErrorMessage is not null)
                return OperationResult.Fail<bool>(outPort.ErrorMessage, outPort.ValidationErrors);

            // Si IsDeleted false y no hay error => resultado válido igualmente
            return OperationResult.Ok(outPort.IsDeleted);
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<bool>(new ErrorInfo($"Error crítico al eliminar el turno: {ex.Message}"), null);
        }
    }
}