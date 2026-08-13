using MedRec.Entity.Enums;

namespace MedRec.Professionals.ViewModels.Models;
public class CreateProfessionalModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Today;
    public ProfessionalType Type { get; set; } = ProfessionalType.Doctor;
    public string? LicenseNumber { get; set; }
    public Guid? SpecialtyId { get; set; }
    public CreateUserForProfessionalModel? CreateUser { get; set; }
}
