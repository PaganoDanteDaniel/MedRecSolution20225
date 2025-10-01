namespace MedRec.Patients.BusinessObjects.DTOs;
public class PatientDetailDto
{
    public PatientDetailDto(Guid patientId,
        string firstName,
        string lastName,
        string documentNumber,
        string address,
        Guid? provinceId,
        Guid? cityId,
        string phoneNumber,
        string email,
        DateTime dateOfBirth,
        Guid? biologicalSexId,
        Guid? insuranceHealthCompanyId,
        string insuranceHealthMemberNumber,
        string insuranceHealthCard,
        string insuranceHealthPlan,
        bool isDeleted,
        byte[] rowVersion)
    {
        PatientId = patientId;
        FirstName = firstName;
        LastName = lastName;
        DocumentNumber = documentNumber;
        Address = address;
        ProvinceId = provinceId;
        CityId = cityId;
        PhoneNumber = phoneNumber;
        Email = email;
        DateOfBirth = dateOfBirth;
        BiologicalSexId = biologicalSexId;
        InsuranceHealthCompanyId = insuranceHealthCompanyId;
        InsuranceHealthMemberNumber = insuranceHealthMemberNumber;
        InsuranceHealthCard = insuranceHealthCard;
        InsuranceHealthPlan = insuranceHealthPlan;
        IsDeleted = isDeleted;
        RowVersion = rowVersion;
    }

    public Guid PatientId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string DocumentNumber { get; }
    public string Address { get; }
    public Guid? ProvinceId { get; }
    public Guid? CityId { get; }
    public string PhoneNumber { get; }
    public string Email { get; }
    public DateTime DateOfBirth { get; }
    public Guid? BiologicalSexId { get; }
    public Guid? InsuranceHealthCompanyId { get; }
    public string InsuranceHealthMemberNumber { get; }
    public string InsuranceHealthCard { get; }
    public string InsuranceHealthPlan { get; }
    public bool IsDeleted { get; } = false;
    public byte[] RowVersion { get; }
}
