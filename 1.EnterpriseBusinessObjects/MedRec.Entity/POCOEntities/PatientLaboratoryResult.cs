using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class PatientLaboratoryResult : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LaboratoryResultId { get; set; }
    public Guid MedicalVisitId { get; set; }
    public DateTime ResultDate { get; set; }
    public string ResultValue { get; set; }
    public string ResultNotes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
