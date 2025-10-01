namespace MedRec.Entity.POCOEntities;
public class PatientMedicalCondition
{
    public Guid Id { get; set; }
    public Guid PatientMedicalHistoryId { get; set; }
    public Guid MedicalConditionId { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
