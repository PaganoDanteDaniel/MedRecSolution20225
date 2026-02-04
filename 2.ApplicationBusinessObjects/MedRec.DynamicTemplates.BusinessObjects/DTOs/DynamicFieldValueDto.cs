namespace MedRec.DynamicTemplates.BusinessObjects.DTOs;

public class DynamicFieldValueDto
{
    public Guid FieldDefinitionId { get; init; }
    public string? FieldValue { get; init; }
    public decimal? NumericValue { get; init; }
    public DateTime? DateValue { get; init; }
    public bool? BooleanValue { get; init; }
}