using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Patients.UseCases.Implementations;

internal class CreatePatientInteractor(
    ICreatePatientOutputPort presenter,
    IPatientCommandsRepository commandRepository,
    IModelValidatorHub<CreatePatientDto> validatorHub) : ICreatePatientInputPort
{
    public async Task HandleAsync(CreatePatientDto dto, CancellationToken cts)
    {
        cts.ThrowIfCancellationRequested();

        // Validación del paciente
        bool isValid = await validatorHub.Validate(dto,
            p => CreatePatientValidator.Validate(p));

        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var patient = (Patient)dto;

        // Crear paciente
        var result = await commandRepository.Create(patient, cts);

        if (!result.IsSuccess)
        {
            await presenter.ErrorAsync(result.Error);
            return;
        }

        await presenter.ErrorAsync(null);
        await presenter.Handle();
    }
}