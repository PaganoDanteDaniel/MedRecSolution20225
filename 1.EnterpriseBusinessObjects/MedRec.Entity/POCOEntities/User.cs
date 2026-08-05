using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class User : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? DoctorId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
