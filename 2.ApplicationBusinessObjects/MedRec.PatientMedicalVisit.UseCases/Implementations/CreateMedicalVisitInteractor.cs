using MedRec.MedicalVisit.BusinessObjects.Repository;
using MedRec.PatientMedicalVisit.BusinessObjects.DTOs;
using MedRec.PatientMedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.PatientMedicalVisit.BusinessObjects.Validator;
using MedRec.Validator.Interfaces;

namespace MedRec.PatientMedicalVisit.UseCases.Implementations;
internal class CreateMedicalVisitInteractor(
    ICreateMedicalVisitOutputPort _outputPort,
    IMedicalVisitCommandRepository _commandRepository,
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
        var medicalVisit = (Entity.POCOEntities.PatientMedicalVisit)dto;
    }
}
