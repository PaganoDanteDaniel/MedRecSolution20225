namespace MedRec.DynamicTemplates.ViewModels.Models;

public class DynamicFieldValueModel
{
    public Guid FieldDefinitionId { get; set; }
    public string? FieldValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BooleanValue { get; set; }

    public DynamicFieldValueModel Clone() => new()
    {
        FieldDefinitionId = FieldDefinitionId,
        FieldValue = FieldValue,
        NumericValue = NumericValue,
        DateValue = DateValue,
        BooleanValue = BooleanValue
    };
}