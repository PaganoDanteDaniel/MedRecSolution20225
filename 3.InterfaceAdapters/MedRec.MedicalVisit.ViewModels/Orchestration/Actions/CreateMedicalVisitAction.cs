using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions;
public class CreateMedicalVisitAction(
    ICreateMedicalVisitInputPort inPort,
    ICreateMedicalVisitOutputPort outPort) :
    ICreateMedicalVisitAction
{
    public async Task<OperationResult<bool>> ExecuteAsync(CreateMedicalVisitModel model, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var dto = MedicalVisitMapper.ToCreateDto(model);
            await inPort.Handle(dto, ct);
            if (outPort.ErrorMessage is not null || outPort.ValidationErrors.Any())
                return OperationResult.Fail<bool>(outPort.ErrorMessage, outPort.ValidationErrors);

            if (!outPort.Created)
                return OperationResult.Unknown<bool>();

            return OperationResult.Ok(outPort.Created);
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<bool>(new ErrorInfo($"Error crítico al crear la visita: {ex.Message}"), null);
        }
    }
}
