using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.Patients.Presenters.Implementations;
internal class PatientForMedicalVisitPresenter :
    BaseOutputPort<PatientForMedicalVisitDto>,
    IPatientForMedicalVisitOutputPort
{
    public Task Handle(Patient patient, HealthInsuranceCompany healthInsurance = null, CancellationToken ct = default)
    {
        if (patient is null)
        {
            Result = OperationResult.Fail<PatientForMedicalVisitDto>(
                new ErrorInfo($"Paciente no encontrado.", ErrorCode.NotFound, new { }, 404), null);
            return Task.CompletedTask;
        }

        var dto = new PatientForMedicalVisitDto
        {
            FullName = $"{patient.LastName}, {patient.FirstName}",
            DateOfBirth = patient.DateOfBirth,
            HealthInsuranceName = healthInsurance?.Name ?? string.Empty,
            Acronym = healthInsurance?.Acronym ?? string.Empty,
            HealthInsuranceCard = patient.HealthInsuranceCard ?? string.Empty,
            HealthInsuranceMemberNumber = patient.HealthInsuranceMemberNumber ?? string.Empty,
            HealthInsurancePlan = patient.HealthInsurancePlan ?? string.Empty
        };

        Result = OperationResult.Ok(dto);

        return Task.CompletedTask;
    }
}
