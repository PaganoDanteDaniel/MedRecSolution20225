using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Patients.UseCases.Implementations;

internal class CreatePatientInteractor(
    ICreatePatientOutputPort _outputPort,
    IPatientCommandsRepository _repository,
    IModelValidatorHub<CreatePatientDto> _validatorHub) : ICreatePatientInputPort
{
    public async Task HandleAsync(CreatePatientDto dto, CancellationToken cts)
    {
        cts.ThrowIfCancellationRequested();

        // Validación del paciente
        bool esValido = await _validatorHub.Validate(dto,
            p => CreatePatientValidator.Validate(p));

        if (!esValido)
        {
            await _outputPort.ValidationErrorsAsync(_validatorHub.Errors);
            return;
        }

        var patient = (Patient)dto;
        // Crear paciente
        var result = await _repository.Create(patient, cts);

        if (!result.IsSuccess)
        {
            await _outputPort.ErrorAsync(result.Error);
            return;
        }

        await _outputPort.ErrorAsync(null);
        await _outputPort.Handle();
    }
}