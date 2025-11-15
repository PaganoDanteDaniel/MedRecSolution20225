using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions;
internal class GetMedicalVisitAction(
    IGetMedicalVisitInputPort inPort,
    IGetMedicalVisitOutputPort outPort) : IGetMedicalVisitAction
{
    public async Task<OperationResult<UpdateMedicalVisitModel>> ExecuteAsync(Guid visitId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            await inPort.Handle(visitId, ct);

            if (outPort.ErrorMessage is not null || outPort.ValidationErrors.Any())
                return OperationResult.Fail<UpdateMedicalVisitModel>(outPort.ErrorMessage, outPort.ValidationErrors);

            if (outPort.MedicalVisit is null)
                return OperationResult.Unknown<UpdateMedicalVisitModel>();

            var model = MedicalVisitMapper.ToUpdateModel(outPort.MedicalVisit);

            return OperationResult.Ok(model);
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<UpdateMedicalVisitModel>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<UpdateMedicalVisitModel>(
                new ErrorInfo($"Error crítico al actualizar la historia clínica del paciente: {ex.Message}"));
        }
    }
}
