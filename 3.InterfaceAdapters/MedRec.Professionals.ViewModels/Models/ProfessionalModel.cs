using MedRec.Entity.Enums;

namespace MedRec.Professionals.ViewModels.Models;
public class ProfessionalModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{LastName}, {FirstName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public ProfessionalType Type { get; set; }
    public string? LicenseNumber { get; set; }
    public Guid? SpecialtyId { get; set; }
}
