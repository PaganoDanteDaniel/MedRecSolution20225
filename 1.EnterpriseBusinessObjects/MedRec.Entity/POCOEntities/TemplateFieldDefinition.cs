namespace MedRec.Entity.POCOEntities;
public class TemplateFieldDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SpecialtyId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty; // Text, Number, Date, etc.
    public string? Category { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? SelectOptions { get; set; } // JSON string
    public string? DefaultValue { get; set; }
    public string? Unit { get; set; }
    public decimal? MinimumValue { get; set; }
    public decimal? MaximumValue { get; set; }
    public string? HelpText { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
