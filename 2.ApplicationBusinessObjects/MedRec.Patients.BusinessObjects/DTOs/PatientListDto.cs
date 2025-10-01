namespace MedRec.Patients.BusinessObjects.DTOs;
public class PatientListDto
{
    public PatientListDto(Guid patientId,
        string firstName,
        string lastName,
        string documentNumber,
        string phoneNumber,
        string email,
        DateTime dateOfBirth)
    {
        PatientId = patientId;
        FirstName = firstName;
        LastName = lastName;
        DocumentNumber = documentNumber;
        PhoneNumber = phoneNumber;
        Email = email;
        DateOfBirth = dateOfBirth;
    }

    public Guid PatientId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string DocumentNumber { get; }
    public string PhoneNumber { get; }
    public string Email { get; }
    public DateTime DateOfBirth { get; }
}
