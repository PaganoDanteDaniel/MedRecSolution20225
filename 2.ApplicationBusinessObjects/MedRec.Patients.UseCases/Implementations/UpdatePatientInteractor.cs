using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.BusinessObjects.Validators;

namespace MedRec.Patients.UseCases.Implementations;

/// <summary>
/// Interactor para actualizar la información de un paciente.
/// </summary>
internal class UpdatePatientInteractor(
    IUpdatePatientOutputPort outputPort,
    IPatientCommandsRepository commandsRepository) : IUpdatePatientInputPort
{
    private readonly IUpdatePatientOutputPort _outputPort = outputPort;
    private readonly IPatientCommandsRepository _commandsRepository = commandsRepository;
    /// <summary>
    /// Maneja la actualización de un paciente.
    /// </summary>
    /// <param name="editPatient">DTO con la información del paciente a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    public async Task Handle(UpdatePatientDto editPatient, CancellationToken cancellationToken = default)
    {

        var patient = (Patient)editPatient ?? throw new ArgumentNullException(nameof(editPatient));



        var errorsValidator = UpdatePatientValidator.Validate(patient);
        if (errorsValidator.Any())
        {
            await _outputPort.ValidationErrorsAsync(errorsValidator);
            return;
        }

        var result = await _commandsRepository.Update(patient);

        if (!result.IsSuccess)
        {
            await _outputPort.ErrorAsync(result.Error);
            return;
        }

        await _outputPort.ErrorAsync(null);
        await _outputPort.Handle(result.Value!);
    }
}

