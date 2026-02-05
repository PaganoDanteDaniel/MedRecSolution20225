using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.Presenters.Implementation;
using MedRec.DynamicTemplates.ViewModels.Models;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration;

/// <summary>
/// Orchestrator for getting template field definitions by specialty
/// </summary>
public class GetTemplateFieldsOrchestrator
{
    private readonly IGetTemplateFieldsBySpecialtyInputPort _inputPort;

    public GetTemplateFieldsOrchestrator(IGetTemplateFieldsBySpecialtyInputPort inputPort)
    {
        _inputPort = inputPort;
    }

    public async Task<(bool Success, List<TemplateFieldDefinitionModel>? Fields, string? ErrorMessage, bool NotFound)>
        ExecuteAsync(Guid specialtyId, CancellationToken cts = default)
    {
        var presenter = new GetTemplateFieldsBySpecialtyPresenter();

        await _inputPort.Handle(specialtyId, cts);

        if (presenter.NotFound)
        {
            return (false, null, presenter.ErrorMessage, true);
        }

        if (!presenter.IsSuccess)
        {
            return (false, null, presenter.ErrorMessage, false);
        }

        var models = presenter.Fields?.Select(dto => new TemplateFieldDefinitionModel
        {
            Id = dto.Id,
            SpecialtyId = dto.SpecialtyId,
            FieldName = dto.FieldName,
            FieldLabel = dto.FieldLabel,
            FieldType = dto.FieldType,
            Category = dto.Category,
            IsRequired = dto.IsRequired,
            DisplayOrder = dto.DisplayOrder,
            SelectOptions = dto.SelectOptions,
            DefaultValue = dto.DefaultValue,
            Unit = dto.Unit,
            MinimumValue = dto.MinimumValue,
            MaximumValue = dto.MaximumValue,
            HelpText = dto.HelpText
        }).ToList();

        return (true, models, null, false);
    }
}