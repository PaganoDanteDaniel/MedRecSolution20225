using MedRec.Entity.Enums;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;

namespace MedRec.Patients.UseCases.Implementations;

internal class PatientDetailsInteractor(
    IPatientDetailsOutputPort outputPort,
    IPatientQueriesRepository queriesRepository) : IPatientDetailsInputPort
{

    private readonly IPatientDetailsOutputPort _outputPort = outputPort;
    private readonly IPatientQueriesRepository _queriesRepository = queriesRepository;

    public async Task Handle(Guid patientId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        try
        {
            // Obtiene los detalles del paciente desde el repositorio.
            var result = await _queriesRepository.GetPatientById(patientId, cts);

            if (!result.IsSuccess)
            {
                await _outputPort.ErrorAsync(result.Error);
                return;
            }

            await _outputPort.Handle(result.Value!);
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
            var result = await _queriesRepository.GetPatientByDocumentNumber(documentNumber, cts);

            // Envía los detalles del paciente al presentador.
            if (!result.IsSuccess)
            {
                await _outputPort.ErrorAsync(result.Error);
                return;
            }

            await _outputPort.Handle(result.Value!);
        }
        catch (Exception)
        {
            throw new BusinessException("Error obteniendo los datos del Paciente", ErrorCode.Unknown);
        }

    }
}
