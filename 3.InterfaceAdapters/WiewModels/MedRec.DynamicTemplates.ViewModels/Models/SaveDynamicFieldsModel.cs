using MedRec.DynamicTemplates.BusinessObjects.DTOs;

namespace MedRec.DynamicTemplates.ViewModels.Models;

public class SaveDynamicFieldsModel
{
    public Guid VisitId { get; set; }
    public List<DynamicFieldValueModel> Fields { get; set; } = [];

    #region Conversion
    public static explicit operator SaveDynamicFieldsDto(SaveDynamicFieldsModel model)
    {
        if (model == null) return null!;

        var fieldDtos = model.Fields.Select(f => new DynamicFieldValueDto
        {
            FieldDefinitionId = f.FieldDefinitionId,
            FieldValue = f.FieldValue,
            NumericValue = f.NumericValue,
            DateValue = f.DateValue,
            BooleanValue = f.BooleanValue
        });

        return new SaveDynamicFieldsDto(model.VisitId, fieldDtos);
    }
    #endregion
}