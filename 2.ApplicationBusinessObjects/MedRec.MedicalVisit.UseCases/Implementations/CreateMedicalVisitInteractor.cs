using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.BusinessObjects.Validator;
using MedRec.Validator.Interfaces;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class CreateMedicalVisitInteractor(
    ICreateMedicalVisitOutputPort _outputPort,
    IMedicalVisitCommandRepository _commandRepository,
    IMedicalVisitQueriesRepository _queryRepository,
    IModelValidatorHub<CreateMedicalVisitDto> _validatorHub) : ICreateMedicalVisitInputPort
{
    public async Task Handle(CreateMedicalVisitDto dto, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        bool isValid = await _validatorHub.Validate(dto,
            v => CreateMedicalVisitValidator.Validate(v));

        if (!isValid)
        {
            await _outputPort.ValidationErrorsAsync(_validatorHub.Errors);
            return;
        }

        var medicalVisit = (PatientMedicalVisit)dto;

        var result = await _commandRepository.Create(medicalVisit, cts);

        if (!result.IsSuccess)
        {
            await _outputPort.ErrorAsync(result.Error);
            return;
        }

        await _outputPort.ErrorAsync(null);
        await _outputPort.Handle();
    }
}
