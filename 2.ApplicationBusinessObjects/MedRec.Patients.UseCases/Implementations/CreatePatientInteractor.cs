using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.BusinessObjects.Validators;

namespace MedRec.Patients.UseCases.Implementations;

internal class CreatePatientInteractor(
    ICreatePatientOutputPort _outputPort,
    IPatientCommandsRepository _repository) : ICreatePatientInputPort
{
    public async Task HandleAsync(CreatePatientDto dto, CancellationToken cts)
    {
        cts.ThrowIfCancellationRequested();

        var patient = (Patient)dto;

        // Validación del paciente
        var errorsValidator = CreatePatientValidator.Validate(patient);
        if (errorsValidator.Any())
        {
            await _outputPort.ValidationErrorsAsync(errorsValidator);
            return;
        }

        // Crear paciente
        var result = await _repository.Create(patient, cts);

        if (!result.IsSuccess)
        {
            await _outputPort.ErrorAsync(result.Error);
            return;
        }

        await _outputPort.ErrorAsync(null);
        await _outputPort.Handle(result.Value!);
    }
}



//internal class CreatePatientInteractor(
//    ICreatePatientOutputPort _outputPort,
//    IPatientCommandsRepository _repository) : ICreatePatientInputPort
//{
//    public async Task HandleAsync(CreatePatientDto dto, CancellationToken cts)
//    {
//        var patient = (Patient)dto;

//        var errorsValidator = CreatePatientValidator.Validate(patient);
//        if (errorsValidator.Any())
//        {
//            await _outputPort.ValidationErrorsAsync(errorsValidator);
//            return;
//        }

//        var result = await _repository.Create(patient);

//        if (!result.IsDeleted)
//        {
//            await _outputPort.ErrorAsync(result.Error);
//            return;
//        }
//        else
//        {
//            await _outputPort.ErrorAsync(string.Empty);
//        }

//        await _outputPort.Handle(patient);
//    }

//}
