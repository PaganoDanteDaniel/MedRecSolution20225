using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Patients.UseCases.Implementations;

internal class PatientForMedicalVisitInteractor : IPatientForMedicalVisitInputPort
{
    private readonly IPatientForMedicalVisitOutputPort _presenter;
    private readonly IPatientQueriesRepository _patientQueries;
    private readonly IHealthInsuranceQueriesRepository _healthInsuranceQueries;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public PatientForMedicalVisitInteractor(
        IPatientForMedicalVisitOutputPort presenter,
        IPatientQueriesRepository patientQueries,
        IHealthInsuranceQueriesRepository healthInsuranceQueries,
        IRepositoryUnitOfWork unitOfWork)
    {
        _presenter = presenter;
        _patientQueries = patientQueries;
        _healthInsuranceQueries = healthInsuranceQueries;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(Guid patientId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        Patient patient = null;
        HealthInsuranceCompany healthInsurance = null;

        await _unitOfWork.ExecuteWithRetry(async () =>
        {
            patient = await _patientQueries.GetPatientById(patientId, ct);
            if (patient is null)
            {
                await _presenter.ErrorAsync(new ErrorInfo(
                    $"El paciente con Id '{patientId}' no existe o fue eliminado.",
                    ErrorCode.NotFound,
                    new { PatientId = patientId },
                    404));
                return;
            }

            if (patient.HealthInsuranceId is Guid insuranceId)
            {
                healthInsurance = await _healthInsuranceQueries.GetById(insuranceId, ct);
            }
        }, ct);

        if (patient is not null)
        {
            await _presenter.ErrorAsync(null);
            await _presenter.Handle(patient, healthInsurance, ct);
        }
    }
}
