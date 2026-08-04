namespace MedRec.DynamicTemplates.ViewModels.Models;

public class TemplateFieldDefinitionModel
{
    public Guid Id { get; set; }
    public Guid SpecialtyId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? SelectOptions { get; set; }
    public string? DefaultValue { get; set; }
    public string? Unit { get; set; }
    public decimal? MinimumValue { get; set; }
    public decimal? MaximumValue { get; set; }
    public string? HelpText { get; set; }
}