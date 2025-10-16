using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.Patients.Presenters.Implementations;
internal class PatientDetailPresenter : IPatientDetailsOutputPort
{
    public PatientDetailDto PatientDetails { get; private set; }

    public IEnumerable<ValidationError> ValidationErrors { get; private set; }

    public ErrorInfo ErrorMessage { get; private set; }

    public Task ErrorAsync(ErrorInfo message)
    {
        ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task Handle(Patient p, HealthInsuranceCompany healthInsurance = null, CancellationToken cancellationToken = default)
    {
        PatientDetails = new PatientDetailDto(
            p.Id,
            p.FirstName,
            p.LastName,
            p.DocumentNumber,
            p.Address,
            p.CityId,
            p.PhoneNumber,
            p.Email,
            p.DateOfBirth,
            p.BiologicalSexId,
            p.HealthInsuranceId,
            healthInsurance?.Name ?? string.Empty,
            p.HealthInsuranceMemberNumber,
            p.HealthInsuranceCard,
            p.HealthInsurancePlan,
            p.RowVersion);

        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        ValidationErrors = errors.ToList();
        return Task.CompletedTask;
    }
}
