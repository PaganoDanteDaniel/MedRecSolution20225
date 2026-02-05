namespace MedRec.DynamicTemplates.ViewModels.Models;

public class DynamicFieldValueModel
{
    public Guid FieldDefinitionId { get; set; }
    public string? FieldValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BooleanValue { get; set; }
}