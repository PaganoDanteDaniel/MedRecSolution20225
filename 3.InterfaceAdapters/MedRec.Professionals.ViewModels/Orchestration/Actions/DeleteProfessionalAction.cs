using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions;

public class DeleteProfessionalAction(
    IDeleteProfessionalInputPort inPort,
    IDeleteProfessionalOutputPort outPort) : IDeleteProfessionalAction
{
    public async Task<OperationResult<bool>> ExecuteAsync(Guid professionalId, CancellationToken ct = default)
    {
        try
        {
            await inPort.HandleAsync(professionalId, ct);
            return outPort.Result;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<bool>(new ErrorInfo($"Error crítico al revertir la creación del profesional: {ex.Message}"), null);
        }
    }
}
