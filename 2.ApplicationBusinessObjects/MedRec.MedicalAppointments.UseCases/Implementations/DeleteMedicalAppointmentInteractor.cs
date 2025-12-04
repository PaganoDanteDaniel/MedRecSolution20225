using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace MedRec.MedicalAppointments.UseCases.Implementations;
internal class DeleteMedicalAppointmentInteractor(
    IDeleteMedicalAppointmentOutputPort presenter,
    IRepositoryUnitOfWork unitOfWork,
    IMedicalAppointmentCommandRepository commandRepository) : IDeleteMedicalAppointmentInputPort
{
    public async Task Handle(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        bool deleted = false;

        try
        {
            await unitOfWork.ExecuteWithRetry(async () =>
            {
                await unitOfWork.BeginTransaction(ct);
                try
                {
                    await commandRepository.Delete(id, ct);
                    var affected = await unitOfWork.SaveChanges(ct);
                    deleted = affected > 0; // idempotente: false si no existía
                    await unitOfWork.CommitTransaction(ct);
                }
                catch
                {
                    await unitOfWork.RollbackTransaction(ct);
                    throw;
                }
            }, ct);

            await presenter.Handle(deleted, ct);
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
                "Conflicto de concurrencia al eliminar el turno.",
                ErrorCode.ConcurrencyError,
                cx.Conflicts,
                409));
        }
        catch (UpdateException ux)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Error al eliminar el turno en la base de datos.",
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
                "Ocurrió un error inesperado al eliminar el turno.",
                ErrorCode.DatabaseError,
                new { Exception = ex.Message },
                500));
        }
    }
}