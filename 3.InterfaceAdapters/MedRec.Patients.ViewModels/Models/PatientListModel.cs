namespace MedRec.Patients.ViewModels.Models;
public class PatientListModel
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DocumentNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }

    public string InformationMessage { get; set; }
}
