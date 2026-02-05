using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.Presenters.Implementation;
using MedRec.DynamicTemplates.ViewModels.Models;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration;

/// <summary>
/// Orchestrator for getting dynamic field values by visit
/// </summary>
public class GetDynamicFieldsOrchestrator
{
    private readonly IGetDynamicFieldsByVisitInputPort _inputPort;

    public GetDynamicFieldsOrchestrator(IGetDynamicFieldsByVisitInputPort inputPort)
    {
        _inputPort = inputPort;
    }

    public async Task<(bool Success, List<DynamicFieldValueModel>? Fields, string? ErrorMessage, bool NotFound)>
        ExecuteAsync(Guid visitId, CancellationToken cts = default)
    {
        var presenter = new GetDynamicFieldsByVisitPresenter();

        await _inputPort.Handle(visitId, cts);

        if (presenter.NotFound)
        {
            return (false, null, presenter.ErrorMessage, true);
        }

        if (!presenter.IsSuccess)
        {
            return (false, null, presenter.ErrorMessage, false);
        }

        var models = presenter.Fields?.Select(dto => new DynamicFieldValueModel
        {
            FieldDefinitionId = dto.FieldDefinitionId,
            FieldValue = dto.FieldValue,
            NumericValue = dto.NumericValue,
            DateValue = dto.DateValue,
            BooleanValue = dto.BooleanValue
        }).ToList();

        return (true, models, null, false);
    }
}