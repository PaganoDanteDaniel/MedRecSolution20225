using MedRec.Entity.Enums;

namespace MedRec.Professionals.ViewModels.Models;
public class UpdateProfessionalModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public ProfessionalType Type { get; set; } = ProfessionalType.Doctor;
    public string? LicenseNumber { get; set; }
    public Guid? SpecialtyId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
