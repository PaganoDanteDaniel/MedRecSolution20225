using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions;

internal class GetMedicalVisitAction(
    IGetMedicalVisitInputPort inPort,
    IGetMedicalVisitOutputPort outPort) : IGetMedicalVisitAction
{
    public async Task<OperationResult<GetMedicalVisitDto>> ExecuteAsync(Guid visitId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            await inPort.Handle(visitId, ct);

            return outPort.Result;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<GetMedicalVisitDto>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<GetMedicalVisitDto>(
                new ErrorInfo($"Error crítico al actualizar la historia clínica del paciente: {ex.Message}"), null);
        }
    }
}
