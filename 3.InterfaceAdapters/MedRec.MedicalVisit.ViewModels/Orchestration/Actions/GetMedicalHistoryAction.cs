using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions;
internal class GetMedicalHistoryAction(
    IGetMedicalHistoryIdInputPort inPort,
    IGetMedicalHistoryIdOutputPort outPort) : IGetMedicalHistoryAction
{
    async Task<OperationResult<Guid>> IGetMedicalHistoryAction.ExecuteAsync(Guid patientId, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            await inPort.Handle(patientId, ct);

            if (outPort.ErrorMessage is not null || outPort.ValidationErrors.Any())
                return OperationResult.Fail<Guid>(outPort.ErrorMessage, outPort.ValidationErrors);

            if (outPort.HistoryId == Guid.Empty)
                return OperationResult.Unknown<Guid>();

            return OperationResult.Ok(outPort.HistoryId);

        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<Guid>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<Guid>(
                new ErrorInfo($"Error crítico al obtener la historia clínica del paciente: {ex.Message}"));
        }
    }
}
