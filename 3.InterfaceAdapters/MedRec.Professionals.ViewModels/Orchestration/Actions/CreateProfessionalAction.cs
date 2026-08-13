using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions;

public class CreateProfessionalAction(
    ICreateProfessionalInputPort inPort,
    ICreateProfessionalOutputPort outPort) : ICreateProfessionalAction
{
    public async Task<OperationResult<Guid>> ExecuteAsync(CreateProfessionalModel model, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var dto = ProfessionalMapper.ToCreateDto(model);
            await inPort.HandleAsync(dto, ct);
            return outPort.Result;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<Guid>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<Guid>(new ErrorInfo($"Error crítico al crear el profesional: {ex.Message}"), null);
        }
    }
}
