using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.BusinessObjects.Validator;
using MedRec.Validator.Interfaces;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class UpdateMedicalVisitInteractor(
    IUpdateMedicalVisitOutputPort outputPort,
    IMedicalVisitCommandRepository commandRepository,
    IModelValidatorHub<UpdateMedicalVisitDto> _validatorHub) : IUpdateMedicalVisitInputPort
{
    public async Task Handle(UpdateMedicalVisitDto dto, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        bool isValid = await _validatorHub.Validate(dto,
            v => UpdateMedicalVisitValidator.Validate(v));
        if (!isValid)
        {
            await outputPort.ValidationErrorsAsync(_validatorHub.Errors);
            return;
        }
        var medicalVisit = (PatientMedicalVisit)dto;

        var result = await commandRepository.Update(medicalVisit, cts);

        if (!result.IsSuccess)
        {
            await outputPort.ErrorAsync(result.Error);
            return;
        }

        await outputPort.ErrorAsync(null);

    }
}
