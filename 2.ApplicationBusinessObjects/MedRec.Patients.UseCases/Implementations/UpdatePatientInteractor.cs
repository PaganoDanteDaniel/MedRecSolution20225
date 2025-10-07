using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Patients.UseCases.Implementations;

/// <summary>
/// Interactor para actualizar la información de un paciente.
/// </summary>
internal class UpdatePatientInteractor(
    IUpdatePatientOutputPort outputPort,
    IPatientCommandsRepository commandsRepository,
    IModelValidatorHub<UpdatePatientDto> validatorHub) : IUpdatePatientInputPort
{
    private readonly IUpdatePatientOutputPort _outputPort = outputPort;
    private readonly IPatientCommandsRepository _commandsRepository = commandsRepository;
    private readonly IModelValidatorHub<UpdatePatientDto> _validatorHub = validatorHub;
    /// <summary>
    /// Maneja la actualización de un paciente.
    /// </summary>
    /// <param name="editPatient">DTO con la información del paciente a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    public async Task Handle(UpdatePatientDto editPatient, CancellationToken cancellationToken = default)
    {
        bool esValido = await _validatorHub.Validate(editPatient, p => UpdatePatientValidator.Validate(p, true));
        if (!esValido)
        {
            await _outputPort.ValidationErrorsAsync(_validatorHub.Errors);
            return;
        }

        var patient = (Patient)editPatient ?? throw new ArgumentNullException(nameof(editPatient));

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

