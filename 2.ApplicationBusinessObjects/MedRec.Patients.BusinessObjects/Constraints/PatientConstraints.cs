namespace MedRec.Patients.BusinessObjects.Constraints;
public static class PatientConstraints
{
    // FirstName
    public const int FirstNameMaxLength = 50;

    // LastName
    public const int LastNameMaxLength = 50;

    // DocumentNumber
    public const int DocumentNumberMinLength = 7;
    public const int DocumentNumberMaxLength = 8;

    // Address
    public const int AddressMaxLength = 100;

    // PhoneNumber
    public const int PhoneNumberMinLength = 6;
    public const int PhoneNumberMaxLength = 20;

    // Email
    public const int EmailMaxLength = 50;

    // Health Insurance fields
    public const int HealthInsuranceMemberNumberMaxLength = 20;
    public const int HealthInsuranceCardMaxLength = 20;
    public const int HealthInsurancePlanMaxLength = 20;
}

