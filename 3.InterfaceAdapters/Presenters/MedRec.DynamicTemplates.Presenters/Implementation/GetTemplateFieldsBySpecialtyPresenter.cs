using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Presenters.Implementation;

/// <summary>
/// Presenter for GetTemplateFieldsBySpecialty use case.
/// </summary>
public class GetTemplateFieldsBySpecialtyPresenter :
    BaseOutputPort<IEnumerable<TemplateFieldDefinitionDto>>, IGetTemplateFieldsBySpecialtyOutputPort
{
    public Task Handle(IEnumerable<TemplateFieldDefinition> fields)
    {
        var dtos = fields.Select(f => new TemplateFieldDefinitionDto
        {
            Id = f.Id,
            SpecialtyId = f.SpecialtyId,
            FieldName = f.FieldName,
            FieldLabel = f.FieldLabel,
            FieldType = f.FieldType,
            Category = f.Category,
            IsRequired = f.IsRequired,
            DisplayOrder = f.DisplayOrder,
            SelectOptions = f.SelectOptions,
            DefaultValue = f.DefaultValue,
            Unit = f.Unit,
            MinimumValue = f.MinimumValue,
            MaximumValue = f.MaximumValue,
            HelpText = f.HelpText
        }).OrderBy(f => f.DisplayOrder).ToList();

        Result = OperationResult<IEnumerable<TemplateFieldDefinitionDto>>.Ok(dtos);
        return Task.CompletedTask;
    }

    public Task HandleNotFound()
    {
        return ErrorAsync(new ErrorInfo(code: ErrorCode.NotFound, message: "No se encontraron campos de plantilla para la especialidad especificada."));
    }
}