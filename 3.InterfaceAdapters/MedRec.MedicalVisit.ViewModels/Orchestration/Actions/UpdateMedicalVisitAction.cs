using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions;

public class UpdateMedicalVisitAction(
    IUpdateMedicalVisitInputPort inPort,
    IUpdateMedicalVisitOutputPort outPort) : IUpdateMedicalVisitAction
{
    public async Task<OperationResult<bool>> ExecuteAsync(UpdateMedicalVisitModel model, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var dto = MedicalVisitMapper.ToUpdateDto(model);
            await inPort.Handle(dto, ct);

            return outPort.Result;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<bool>(new ErrorInfo($"Error crítico al actualizar la visita: {ex.Message}"), null);
        }
    }
}
