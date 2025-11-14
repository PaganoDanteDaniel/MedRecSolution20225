using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace MedRec.MedicalAppointments.UseCases.Implementations;

internal class MoveMedicalAppointmentInteractor(
    IRepositoryUnitOfWork unitOfWork,
    IMoveMedicalAppointmentOutputPort presenter,
    IMedicalAppointmentCommandRepository commandsRepository,
    IMedicalAppointmentQueriesRepository queriesRepository) : IMoveMedicalAppointmentInputPort
{
    public async Task Handle(MoveMedicalAppointmentDto moveAppointmentDto, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            await unitOfWork.ExecuteWithRetryAsync(async () =>
            {
                await unitOfWork.BeginTransaction(ct);
                try
                {
                    // Construye la entidad mínima requerida por el comando Move (Id, AppointmentDateTime, RowVersion)
                    var entity = new MedicalAppointment
                    {
                        Id = moveAppointmentDto.Id,
                        DateTime = moveAppointmentDto.DateTime,
                        RowVersion = moveAppointmentDto.RowVersion
                    };

                    await commandsRepository.Move(entity, ct);
                    await unitOfWork.SaveChanges(ct);
                    await unitOfWork.CommitTransaction(ct);
                }
                catch
                {
                    await unitOfWork.RollbackTransaction(ct);
                    throw;
                }
            }, ct);

            // Leer el turno actualizado (RowVersion y demás campos actuales)
            var updated = await queriesRepository.GetById(moveAppointmentDto.Id, ct);
            await presenter.Handle(updated, ct);
        }
        catch (LostConnectionException lce)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                lce.Message,
                ErrorCode.DatabaseError,
                503));
        }
        catch (ConcurrencyException cx)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Conflicto de concurrencia al mover el turno.",
                ErrorCode.ConcurrencyError,
                cx.Conflicts,
                409));
        }
        catch (DuplicateKeyException dx)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Conflicto por restricción de unicidad al mover el turno.",
                ErrorCode.DuplicateKey,
                dx.Details,
                409));
        }
        catch (UpdateException ux)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Error al persistir cambios al mover el turno.",
                ErrorCode.UpdateError,
                ux.Details,
                500));
        }
        catch (BusinessException bx)
        {
            await presenter.ErrorAsync(bx.Error);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Ocurrió un error inesperado al mover el turno.",
                ErrorCode.Unknown,
                new { Exception = ex.Message },
                500));
        }
    }
}
