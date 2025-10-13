namespace MedRec.Entity.POCOEntities;
public class PatientLaboratoryResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LaboratoryResultId { get; set; }
    public Guid MedicalVisitId { get; set; }
    public DateTime ResultDate { get; set; }
    public string ResultValue { get; set; }
    public string ResultNotes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
