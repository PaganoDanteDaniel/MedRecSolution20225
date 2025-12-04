using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.BusinessObjects.Validator;
using MedRec.Shared.DTOs;
using MedRec.Shared.Exceptions.SQLExceptions;
using MedRec.Validator.Interfaces;

namespace MedRec.MedicalVisit.UseCases.Implementations;
public class UpdateMedicalVisitInteractorUoW(
    IUpdateMedicalVisitOutputPort outputPort,
    IMedicalVisitCommandRepositoryUoW commandRepository,
    IMedicalVisitQueriesRepositoryUoW queriesRepository,
    IModelValidatorHub<UpdateMedicalVisitDto> validatorHub,
    IRepositoryUnitOfWork unitOfWork) : IUpdateMedicalVisitInputPort
{
    public async Task Handle(UpdateMedicalVisitDto dto, CancellationToken ct = default)
    {
        if (!await validatorHub.Validate(dto, v => UpdateMedicalVisitValidator.Validate(v)))
        {
            await outputPort.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        try
        {
            ct.ThrowIfCancellationRequested();
            await unitOfWork.BeginTransaction(ct);

            var updateMedicalVisit = (PatientMedicalVisit)dto;

            await commandRepository.Update(updateMedicalVisit, ct);
            var response = await unitOfWork.SaveChanges(ct);
            await unitOfWork.CommitTransaction(ct);

            await outputPort.Handle(response > 0, ct);
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

            var data = await queriesRepository.GetMedicalVisit(dto.Id, ct);

            var fixedConflicts = ex.Conflicts
                .Select(conflict =>
                {
                    if (conflict.PropertyName == "RowVersion")
                    {
                        return new ConcurrencyConflictDto(
                            conflict.EntityName,
                            conflict.PropertyName,
                            conflict.DataBaseValue,
                            userValue: data.RowVersion
                        );
                    }
                    return conflict;
                })
                .ToList();

            await outputPort.ErrorAsync(new ErrorInfo(
                message: "Este registro fue actualizado por otro usuario. <br />Se han cargado los datos más recientes. <br />Revise y guarde nuevamente si es necesario.",
                code: ErrorCode.ConcurrencyError,
                details: new
                {
                    ExceptionType = nameof(ConcurrencyException),
                    Conflicts = fixedConflicts,
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
                    ex.StackTrace
                },
                httpStatusCode: 500
            ));
        }
    }
}
