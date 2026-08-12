using MedRec.Entity.Enums;

namespace MedRec.Professionals.BusinessObjects.DTOs;
public class CreateProfessionalDto
{
    public CreateProfessionalDto(
        string firstName,
        string lastName,
        string email,
        string? phone,
        DateTime hireDate,
        ProfessionalType type,
        string? licenseNumber,
        Guid? specialtyId)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        HireDate = hireDate;
        Type = type;
        LicenseNumber = licenseNumber;
        SpecialtyId = specialtyId;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? Phone { get; }
    public DateTime HireDate { get; }
    public ProfessionalType Type { get; }
    public string? LicenseNumber { get; }
    public Guid? SpecialtyId { get; }
}
