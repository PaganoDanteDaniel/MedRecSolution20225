namespace MedRec.Entity.POCOEntities;
public class PatientMedicalHistory
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Notes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
