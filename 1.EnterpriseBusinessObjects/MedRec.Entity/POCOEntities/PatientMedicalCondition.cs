using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class PatientMedicalCondition : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientMedicalHistoryId { get; set; }
    public Guid MedicalConditionId { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
