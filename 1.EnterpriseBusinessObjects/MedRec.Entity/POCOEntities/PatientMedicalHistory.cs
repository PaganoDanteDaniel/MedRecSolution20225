namespace MedRec.Entity.POCOEntities;
public class PatientMedicalHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public string Notes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
