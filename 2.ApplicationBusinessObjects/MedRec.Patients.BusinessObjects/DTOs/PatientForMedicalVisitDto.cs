namespace MedRec.Patients.BusinessObjects.DTOs;
public class PatientForMedicalVisitDto
{
    public string FullName { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string HealthInsuranceName { get; init; }
    public string Acronym { get; init; }
    public string HealthInsuranceMemberNumber { get; init; }
    public string HealthInsuranceCard { get; init; }
    public string HealthInsurancePlan { get; init; }
}
