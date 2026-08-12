using MedRec.Entity.Enums;

namespace MedRec.Professionals.BusinessObjects.DTOs;
public class UpdateProfessionalDto
{
    public UpdateProfessionalDto(
        Guid id,
        string firstName,
        string lastName,
        string phone,
        ProfessionalType type,
        string? licenseNumber,
        Guid? specialtyId,
        byte[] rowVersion)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Type = type;
        LicenseNumber = licenseNumber;
        SpecialtyId = specialtyId;
        RowVersion = rowVersion;
    }

    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Phone { get; }
    public ProfessionalType Type { get; }
    public string? LicenseNumber { get; }
    public Guid? SpecialtyId { get; }
    public byte[] RowVersion { get; }
}
