using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;

namespace MedRec.Patients.UseCases.Implementations;

internal class PatientDetailsInteractor(
    IPatientDetailsOutputPort outputPort,
    IPatientQueriesRepository queriesRepository,
    IHealthInsuranceQueriesRepository healthInsuranceQueriesRepository) : IPatientDetailsInputPort
{

    private readonly IPatientDetailsOutputPort _outputPort = outputPort;
    private readonly IPatientQueriesRepository _queriesRepository = queriesRepository;
    private readonly IHealthInsuranceQueriesRepository _healthInsuranceQueriesRepository = healthInsuranceQueriesRepository;
    public async Task Handle(Guid patientId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        try
        {
            // Obtiene los detalles del paciente desde el repositorio.
            var patientResult = await _queriesRepository.GetPatientById(patientId, cts);



            if (!patientResult.IsSuccess)
            {
                await _outputPort.ErrorAsync(patientResult.Error);
                return;
            }

            Result<HealthInsuranceCompany> healthResult = null;

            if (patientResult?.Value.HealthInsuranceId is Guid id)
            {
                healthResult = await _healthInsuranceQueriesRepository.GetById(id, cts);
                await _outputPort.Handle(patientResult.Value!, healthResult.Value);
                return;
            }
            await _outputPort.Handle(patientResult.Value!);

        }
        catch (Exception)
        {
            throw new BusinessException("Error obteniendo los datos del Paciente", ErrorCode.Unknown);
        }

    }

    public async Task Handle(string documentNumber, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        try
        {
            // Obtiene los detalles del paciente desde el repositorio.
            var patientResult = await _queriesRepository.GetPatientByDocumentNumber(documentNumber, cts);

            // Envía los detalles del paciente al presentador.
            if (!patientResult.IsSuccess)
            {
                await _outputPort.ErrorAsync(patientResult.Error);
                return;
            }

            Result<HealthInsuranceCompany> healthResult = null;

            if (patientResult?.Value.HealthInsuranceId is Guid id)
            {
                healthResult = await _healthInsuranceQueriesRepository.GetById(id, cts);
                await _outputPort.Handle(patientResult.Value!, healthResult.Value);
                return;
            }
            await _outputPort.Handle(patientResult.Value!);
        }
        catch (Exception)
        {
            throw new BusinessException("Error obteniendo los datos del Paciente", ErrorCode.Unknown);
        }

    }
}
