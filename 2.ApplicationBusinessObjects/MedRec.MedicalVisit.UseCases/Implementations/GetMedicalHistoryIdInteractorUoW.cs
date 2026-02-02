using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace MedRec.MedicalVisit.UseCases.Implementations;
public class GetMedicalHistoryIdInteractorUoW(
    IGetMedicalHistoryIdOutputPort _outputPort,
    IMedicalVisitQueriesRepositoryUoW _repository,
    IMedicalVisitCommandRepositoryUoW _command,
    IRepositoryUnitOfWork unitOfWork) : IGetMedicalHistoryIdInputPort
{
    public async Task Handle(Guid patientId, CancellationToken ct = default)
    {
        if (patientId == Guid.Empty)
        {
            await _outputPort.ErrorAsync(new ErrorInfo(
                message: "El ID del paciente es inválido.",
                code: ErrorCode.ValidationError,
                httpStatusCode: 400
            ));
            return;
        }

        try
        {
            // 1. Intentar obtener historial existente
            Guid existingId = await _repository.GetMedicalHistory(patientId, ct);
            if (existingId != Guid.Empty)
            {
                await _outputPort.Handle(existingId, ct);
                return;
            }

            // 2. No existe → crear dentro de transacción
            await unitOfWork.BeginTransaction(ct);

            try
            {
                Guid newId = await _command.CreateMedicalHistory(patientId, ct);
                await unitOfWork.SaveChanges(ct);
                await unitOfWork.CommitTransaction(ct);
                await _outputPort.Handle(newId, ct);
            }
            catch (DuplicateKeyException)
            {
                // Otro hilo creó el historial → rollback y volver a leer
                await unitOfWork.RollbackTransaction(ct);
                existingId = await _repository.GetMedicalHistory(patientId, ct);
                if (existingId != Guid.Empty)
                {
                    await _outputPort.Handle(existingId, ct);
                }
                else
                {
                    await _outputPort.ErrorAsync(new ErrorInfo(
                        message: "Conflicto al crear el historial clínico: no se pudo recuperar el ID.",
                        code: ErrorCode.DuplicateKey,
                        httpStatusCode: 409
                    ));
                }
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackTransaction(ct);
                await _outputPort.ErrorAsync(new ErrorInfo(
                    message: "Error al crear el historial clínico.",
                    code: ErrorCode.DatabaseError,
                    details: ex,
                    httpStatusCode: 500
                ));
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelación intencionada: no es error de dominio
            throw;
        }
        catch (Exception ex)
        {
            await _outputPort.ErrorAsync(new ErrorInfo(
                message: "Error inesperado al gestionar el historial clínico.",
                code: ErrorCode.Unknown,
                details: ex,
                httpStatusCode: 500
            ));
        }
    }
}
