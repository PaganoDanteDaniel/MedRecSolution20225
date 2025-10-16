using MedRec.Entity.Enums;

namespace MedRec.Patients.BusinessObjects.DTOs;
public class PatientDetailDto
{
    public PatientDetailDto(
        Guid Id,
        string firstName,
        string lastName,
        string documentNumber,
        string address,
        Guid? cityId,
        string phoneNumber,
        string email,
        DateTime dateOfBirth,
        BiologicalSex biologicalSexId,
        Guid? healthInsuranceCompanyId,
        string healthInsuranceCompanyName,
        string healthInsuranceMemberNumber,
        string healthInsuranceCard,
        string healthInsurancePlan,
        byte[] rowVersion)
    {
        this.Id = Id;
        FirstName = firstName;
        LastName = lastName;
        DocumentNumber = documentNumber;
        Address = address;
        CityId = cityId;
        PhoneNumber = phoneNumber;
        Email = email;
        DateOfBirth = dateOfBirth;
        BiologicalSexId = biologicalSexId;
        HealthInsuranceCompanyId = healthInsuranceCompanyId;
        HealthInsuranceCompanyName = healthInsuranceCompanyName;
        HealthInsuranceMemberNumber = healthInsuranceMemberNumber;
        HealthInsuranceCard = healthInsuranceCard;
        HealthInsurancePlan = healthInsurancePlan;
        RowVersion = rowVersion;
    }

    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string DocumentNumber { get; }
    public string Address { get; }
    public Guid? CityId { get; }
    public string PhoneNumber { get; }
    public string Email { get; }
    public DateTime DateOfBirth { get; }
    public BiologicalSex BiologicalSexId { get; }
    public Guid? HealthInsuranceCompanyId { get; }
    public string HealthInsuranceCompanyName { get; }
    public string HealthInsuranceMemberNumber { get; }
    public string HealthInsuranceCard { get; }
    public string HealthInsurancePlan { get; }
    public bool IsDeleted { get; } = false;
    public byte[] RowVersion { get; }
}
