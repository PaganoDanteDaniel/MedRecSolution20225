using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.BusinessObjects.Validator;
using MedRec.Validator.Interfaces;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class CreateMedicalVisitInteractor(
    ICreateMedicalVisitOutputPort outputPort,
    IMedicalVisitCommandRepository commandRepository,
    IModelValidatorHub<MedicalVisitDto> validatorHub) : ICreateMedicalVisitInputPort
{
    public async Task Handle(MedicalVisitDto dto, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        bool isValid = await validatorHub.Validate(dto,
            v => CreateMedicalVisitValidator.Validate(v));

        if (!isValid)
        {
            await outputPort.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var medicalVisit = (PatientMedicalVisit)dto;

        var result = await commandRepository.Create(medicalVisit, cts);

        if (!result.IsSuccess)
        {
            await outputPort.ErrorAsync(result.Error);
            return;
        }

        await outputPort.ErrorAsync(null);
        await outputPort.Handle();
    }
}
