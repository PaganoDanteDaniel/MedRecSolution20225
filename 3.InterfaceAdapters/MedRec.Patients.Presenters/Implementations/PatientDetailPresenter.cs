using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.Patients.Presenters.Implementations;
internal class PatientDetailPresenter :
    BaseOutputPort<PatientDetailDto>,
    IPatientDetailsOutputPort
{
    private PatientDetailDto _patientDetails;

    public Task Handle(Patient patient, HealthInsuranceCompany healthInsurance = null, CancellationToken cancellationToken = default)
    {
        if (patient != null)
        {
            _patientDetails = new PatientDetailDto(
                patient.Id,
                patient.FirstName,
                patient.LastName,
                patient.DocumentNumber,
                patient.Address,
                patient.CityId,
                patient.PhoneNumber,
                patient.Email,
                patient.DateOfBirth,
                patient.BiologicalSexId,
                patient.HealthInsuranceId,
                healthInsurance?.Name ?? string.Empty,
                patient.HealthInsuranceMemberNumber,
                patient.HealthInsuranceCard,
                patient.HealthInsurancePlan,
                patient.RowVersion);

            Result = OperationResult<PatientDetailDto>.Ok(_patientDetails);
        }

        return Task.CompletedTask;
    }
}
