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
    public async Task<OperationResult<Guid>> ExecuteAsync(CreateMedicalVisitModel model, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var dto = MedicalVisitMapper.ToCreateDto(model);
            await inPort.Handle(dto, ct);

            return outPort.Result;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<Guid>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<Guid>(new ErrorInfo($"Error crítico al crear la visita: {ex.Message}"), null);
        }
    }
}
