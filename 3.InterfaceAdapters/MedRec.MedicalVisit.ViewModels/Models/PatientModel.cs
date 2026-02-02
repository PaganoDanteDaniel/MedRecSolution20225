namespace MedRec.MedicalVisit.ViewModels.Models;
public class PatientModel
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string HealthInsuranceName { get; set; }
    public string Acronym { get; set; }
    public string HealthInsuranceMemberNumber { get; set; }
    public string HealthInsuranceCard { get; set; }
    public string HealthInsurancePlan { get; set; }
}
