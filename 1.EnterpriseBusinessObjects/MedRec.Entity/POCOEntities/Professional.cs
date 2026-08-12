using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;

public class Professional : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ProfessionalType Type { get; set; }
    public string? LicenseNumber { get; set; }
    public Guid? SpecialtyId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string FullName => $"{LastName}, {FirstName}";
}
