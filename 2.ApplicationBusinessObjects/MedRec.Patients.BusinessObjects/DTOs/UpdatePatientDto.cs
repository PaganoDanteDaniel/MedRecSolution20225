using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;

namespace MedRec.Patients.BusinessObjects.DTOs;
public class UpdatePatientDto
{
    public UpdatePatientDto(
        Guid patientId,
        string firstName,
        string lastName,
        string documentNumber,
        string address = "",
        Guid? provinceId = null,
        Guid? cityId = null,
        string phoneNumber = "",
        string email = "",
        DateTime dateOfBirth = default,
        BiologicalSex biologicalSex = BiologicalSex.Unknown,
        Guid? healthInsuranceId = null,
        string healthInsuranceMemberNumber = "",
        string healthInsuranceCard = "",
        string healthInsurancePlan = "",
        byte[] rowVersion = null)
    {
        Id = patientId;
        FirstName = firstName;
        LastName = lastName;
        DocumentNumber = documentNumber;
        Address = address ?? string.Empty;
        ProvinceId = provinceId;
        CityId = cityId;
        PhoneNumber = phoneNumber ?? string.Empty;
        Email = email ?? string.Empty;
        DateOfBirth = dateOfBirth;
        BiologicalSexId = biologicalSex;
        HealthInsuranceId = healthInsuranceId;
        HealthInsuranceMemberNumber = healthInsuranceMemberNumber ?? string.Empty;
        HealthInsuranceCard = healthInsuranceCard ?? string.Empty;
        HealthInsurancePlan = healthInsurancePlan ?? string.Empty;
        RowVersion = rowVersion;
    }
    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string DocumentNumber { get; }
    public string Address { get; }
    public Guid? ProvinceId { get; }
    public Guid? CityId { get; }
    public string PhoneNumber { get; }
    public string Email { get; }
    public DateTime DateOfBirth { get; }
    public BiologicalSex BiologicalSexId { get; }
    public Guid? HealthInsuranceId { get; }
    public string HealthInsuranceMemberNumber { get; }
    public string HealthInsuranceCard { get; }
    public string HealthInsurancePlan { get; }
    public byte[] RowVersion { get; }

    public static explicit operator Patient(UpdatePatientDto updatePatientDto)
    {
        return new Patient
        {
            Id = updatePatientDto.Id,
            FirstName = updatePatientDto.FirstName,
            LastName = updatePatientDto.LastName,
            DocumentNumber = updatePatientDto.DocumentNumber,
            Address = updatePatientDto.Address,
            ProvinceId = updatePatientDto.ProvinceId,
            CityId = updatePatientDto.CityId,
            PhoneNumber = updatePatientDto.PhoneNumber,
            Email = updatePatientDto.Email,
            DateOfBirth = updatePatientDto.DateOfBirth,
            BiologicalSexId = updatePatientDto.BiologicalSexId,
            HealthInsuranceId = updatePatientDto.HealthInsuranceId,
            HealthInsuranceMemberNumber = updatePatientDto.HealthInsuranceMemberNumber,
            HealthInsuranceCard = updatePatientDto.HealthInsuranceCard,
            HealthInsurancePlan = updatePatientDto.HealthInsurancePlan,
            RowVersion = updatePatientDto.RowVersion
        };
    }
}
