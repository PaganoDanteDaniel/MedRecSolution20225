using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions;

internal class GetTemplateFieldsAction(
    IGetTemplateFieldsBySpecialtyInputPort inputPort,
    IGetTemplateFieldsBySpecialtyOutputPort outputPort) : IGetTemplateFieldsAction
{
    public async Task<OperationResult<IEnumerable<TemplateFieldDefinitionDto>>> ExecuteAsync(Guid specialtyId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        try
        {
            await inputPort.Handle(specialtyId, cts);

            return outputPort.Result;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<IEnumerable<TemplateFieldDefinitionDto>>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<IEnumerable<TemplateFieldDefinitionDto>>(
                new ErrorInfo($"Error crítico al actualizar la historia clínica del paciente: {ex.Message}"), null);
        }
    }
}
