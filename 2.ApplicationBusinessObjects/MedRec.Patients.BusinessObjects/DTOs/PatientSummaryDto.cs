namespace MedRec.Patients.BusinessObjects.DTOs;
public class PatientSummaryDto
{
    public PatientSummaryDto(Guid id,
        string firstName,
        string lastName,
        string documentNumber,
        string phoneNumber,
        string email,
        DateTime dateOfBirth)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        DocumentNumber = documentNumber;
        PhoneNumber = phoneNumber;
        Email = email;
        DateOfBirth = dateOfBirth;
    }

    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string DocumentNumber { get; }
    public string PhoneNumber { get; }
    public string Email { get; }
    public DateTime DateOfBirth { get; }
}
