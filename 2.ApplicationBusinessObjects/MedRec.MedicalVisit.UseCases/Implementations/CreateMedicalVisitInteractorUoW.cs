using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.BusinessObjects.Validator;
using MedRec.Shared.Exceptions.SQLExceptions;
using MedRec.Validator.Interfaces;

namespace MedRec.MedicalVisit.UseCases.Implementations;
public class CreateMedicalVisitInteractorUoW(
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

        try
        {
            ct.ThrowIfCancellationRequested();
            // 2. Iniciar transacción
            await unitOfWork.BeginTransaction(ct);


            // 3. Crear entidad desde DTO
            var medicalVisit = (PatientMedicalVisit)dto;

            // 4. Persistir
            await commandRepository.Create(medicalVisit, ct);
            await unitOfWork.SaveChanges(ct);
            await unitOfWork.CommitTransaction(ct);

            // 5. Éxito
            await outputPort.Handle();
        }
        catch (DuplicateKeyException ex)
        {
            await unitOfWork.RollbackTransaction(ct);
            await outputPort.ErrorAsync(new ErrorInfo(
                message: "Ya existe una visita médica con estos datos únicos.",
                code: ErrorCode.DuplicateKey,
                details: new
                {
                    ExceptionType = nameof(DuplicateKeyException),
                    Entities = ex.Entities?.ToArray(),
                    InnerMessage = ex.InnerException?.Message
                },
                httpStatusCode: 409
            ));
        }
        catch (ConcurrencyException ex)
        {
            await unitOfWork.RollbackTransaction(ct);
            await outputPort.ErrorAsync(new ErrorInfo(
                message: "Conflicto de concurrencia al crear la visita médica.",
                code: ErrorCode.ConcurrencyError,
                details: new
                {
                    ExceptionType = nameof(ConcurrencyException),
                    Conflicts = ex.Conflicts,
                    InnerMessage = ex.InnerException?.Message
                },
                httpStatusCode: 409
            ));
        }
        catch (OperationCanceledException)
        {
            await unitOfWork.RollbackTransaction(CancellationToken.None);
            // Cancelación: no es un error de dominio, se re-lanza
            throw;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransaction(ct);
            await outputPort.ErrorAsync(new ErrorInfo(
                message: "Error inesperado al crear la visita médica.",
                code: ErrorCode.DatabaseError,
                details: new
                {
                    ExceptionType = ex.GetType().Name,
                    InnerMessage = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                },
                httpStatusCode: 500
            ));
        }
    }
}
