using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.Patients.Presenters.Implementations;
internal class PatientForMedicalVisitPresenter : IPatientForMedicalVisitOutputPort
{
    public PatientForMedicalVisitDto DataPatient { get; private set; }

    public IEnumerable<ValidationError> ValidationErrors { get; private set; } = [];

    public ErrorInfo ErrorMessage { get; private set; }

    public Task ErrorAsync(ErrorInfo message)
    {
        ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task Handle(Patient dataPatient, HealthInsuranceCompany healthInsurance = null, CancellationToken ct = default)
    {
        DataPatient = new PatientForMedicalVisitDto
        {
            FullName = $"{dataPatient.LastName}, {dataPatient.FirstName}",
            DateOfBirth = dataPatient.DateOfBirth,
            HealthInsuranceName = healthInsurance?.Name ?? string.Empty,
            Acronym = healthInsurance?.Acronym ?? string.Empty,
            HealthInsuranceCard = dataPatient.HealthInsuranceCard ?? string.Empty,
            HealthInsuranceMemberNumber = dataPatient.HealthInsuranceMemberNumber ?? string.Empty,
            HealthInsurancePlan = dataPatient.HealthInsurancePlan ?? string.Empty
        };
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        ValidationErrors = errors.ToList();
        return Task.CompletedTask;
    }
}
