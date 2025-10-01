using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Patients.UseCases.Implementations;
/// <summary>
/// Interactor para manejar la eliminación de un paciente.
/// </summary>
/// 
/// <param name="_outputPort">El presentador para notificar los resultados.</param>
/// <param name="_commandRepository">La unidad de trabajo para manejar la eliminación del paciente.</param>
internal class DeletePatientInteractor(
    IDeletePatientOutputPort outputPort,
    IPatientCommandsRepository commandRepository,
    IPatientQueriesRepository queriesRepository) : IDeletePatientInputPort
{
    private readonly IDeletePatientOutputPort _outputPort = outputPort;
    private readonly IPatientCommandsRepository _commandRepository = commandRepository;
    private readonly IPatientQueriesRepository _queriesRepository = queriesRepository;
    /// <summary>
    /// Maneja la lógica para eliminar un paciente.
    /// </summary>
    /// <param name="deletePatient">El ID del paciente a eliminar.</param>
    public async Task Handle(Guid deletePatient, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        // Obtiene los detalles del paciente a eliminar.
        var getResult = await _queriesRepository.GetPatientById(deletePatient, cts);

        if (!getResult.IsSuccess)
        {
            // Error de infraestructura, conexión, timeout, etc.
            await _outputPort.ErrorAsync(getResult.Error!); // O mapea a un error genérico
            return;
        }
        var patient = getResult.Value;

        if (patient.IsDeleted)
        {
            await _outputPort.ErrorAsync(new ErrorInfo("El paciente ya fue eliminado", ErrorCode.Conflict));
            return;
        }

        //Marca al paciente como eliminado.
        patient.IsDeleted = true;

        // Elimina al paciente.
        var deleteResult = await _commandRepository.SoftDelete(patient, cts);

        // Si falló el guardado
        if (!deleteResult.IsSuccess)
        {
            await _outputPort.ErrorAsync(deleteResult.Error!);
            return;
        }

        // Éxito
        await _outputPort.Handle(deleteResult.Value);

    }
}
