namespace MedRec.DynamicTemplates.BusinessObjects.DTOs;

public class TemplateFieldDefinitionDto
{
    public Guid Id { get; init; }
    public Guid SpecialtyId { get; init; }
    public string FieldName { get; init; } = string.Empty;
    public string FieldLabel { get; init; } = string.Empty;
    public string FieldType { get; init; } = string.Empty;
    public string? Category { get; init; }
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public string? SelectOptions { get; init; } // JSON
    public string? DefaultValue { get; init; }
    public string? Unit { get; init; }
    public decimal? MinimumValue { get; init; }
    public decimal? MaximumValue { get; init; }
    public string? HelpText { get; init; }
}