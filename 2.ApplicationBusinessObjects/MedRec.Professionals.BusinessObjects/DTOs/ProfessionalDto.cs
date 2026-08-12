using MedRec.Entity.Enums;

namespace MedRec.Professionals.BusinessObjects.DTOs;
public class ProfessionalDto
{
    public ProfessionalDto(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime hireDate,
        ProfessionalType type,
        string? licenseNumber,
        Guid? specialtyId,
        byte[] rowVersion)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        HireDate = hireDate;
        Type = type;
        LicenseNumber = licenseNumber;
        SpecialtyId = specialtyId;
        RowVersion = rowVersion;
    }

    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string FullName => $"{LastName}, {FirstName}";
    public string Email { get; }
    public string Phone { get; }
    public DateTime HireDate { get; }
    public ProfessionalType Type { get; }
    public string? LicenseNumber { get; }
    public Guid? SpecialtyId { get; }
    public byte[] RowVersion { get; }
}
