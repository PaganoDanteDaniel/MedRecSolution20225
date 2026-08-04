namespace MedRec.Entity.POCOEntities;

public class MedicalVisitDynamicField
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientMedicalVisitId { get; set; }
    public Guid FieldDefinitionId { get; set; }
    public string? FieldValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BooleanValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
