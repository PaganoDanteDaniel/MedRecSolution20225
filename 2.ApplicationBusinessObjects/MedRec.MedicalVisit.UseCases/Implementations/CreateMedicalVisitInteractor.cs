using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.BusinessObjects.Validator;
using MedRec.Validator.Interfaces;

namespace MedRec.MedicalVisit.UseCases.Implementations;

public class CreateMedicalVisitInteractor(
    ICreateMedicalVisitOutputPort outputPort,
    IMedicalVisitCommandRepositoryUoW commandRepository,
    IModelValidatorHub<CreateMedicalVisitDto> validatorHub,
    IRepositoryUnitOfWork unitOfWork) : ICreateMedicalVisitInputPort
{
    public async Task Handle(CreateMedicalVisitDto dto, CancellationToken ct = default)
    {
        // 1. Validar el DTO
        if (!await validatorHub.Validate(dto, v => CreateMedicalVisitValidator.Validate(v)))
        {
            await outputPort.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }


        ct.ThrowIfCancellationRequested();
        // 2. Iniciar transacción
        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            // 3. Crear entidad desde DTO
            var medicalVisit = (PatientMedicalVisit)dto;

            // 4. Persistir
            await commandRepository.Create(medicalVisit, ct);
            await unitOfWork.SaveChanges(ct);

            // 5. Éxito
            await outputPort.ErrorAsync(null);
            await outputPort.Handle(medicalVisit);
        }, ct);
    }
}
