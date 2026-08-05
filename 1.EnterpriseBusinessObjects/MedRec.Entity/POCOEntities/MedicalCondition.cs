using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class MedicalCondition : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConditionTypeId { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
