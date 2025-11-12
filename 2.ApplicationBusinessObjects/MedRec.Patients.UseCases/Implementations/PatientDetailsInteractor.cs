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
    IPatientQueriesRepository patientQueries,
    IHealthInsuranceQueriesRepository healthInsuranceQueries) : IPatientDetailsInputPort
{
    public async Task Handle(Guid patientId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        try
        {
            // Obtiene los detalles del paciente desde el repositorio.
            var patientResult = await patientQueries.GetPatientById(patientId, cts);

            if (!patientResult.IsSuccess)
            {
                await outputPort.ErrorAsync(patientResult.Error);
                return;
            }

            if (patientResult.Value.HealthInsuranceId.HasValue)
            {
                await GetHealthInsurance(patientResult, cts);
                return;
            }
            await outputPort.Handle(patientResult.Value!);

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
            var patientResult = await patientQueries.GetPatientByDocumentNumber(documentNumber, cts);

            // Envía los detalles del paciente al presentador.
            if (!patientResult.IsSuccess)
            {
                await outputPort.ErrorAsync(patientResult.Error);
                return;
            }

            if (patientResult.Value.HealthInsuranceId.HasValue)
            {
                await GetHealthInsurance(patientResult, cts);
                return;
            }
            await outputPort.Handle(patientResult.Value!);
        }
        catch (Exception)
        {
            throw new BusinessException("Error obteniendo los datos del Paciente", ErrorCode.Unknown);
        }

    }

    private async Task GetHealthInsurance(Result<Patient> patientResult, CancellationToken cts)
    {
        cts.ThrowIfCancellationRequested();
        try
        {
            if (patientResult.Value.HealthInsuranceId is Guid id)
            {
                var healthResult = await healthInsuranceQueries.GetById(id, cts);
                await outputPort.Handle(patientResult.Value!, healthResult);
            }
        }
        catch (Exception)
        {
            throw;
        }

    }
}
