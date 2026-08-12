using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class MedicalAppointment : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime DateTime { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProfessionalId { get; set; } = Guid.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}