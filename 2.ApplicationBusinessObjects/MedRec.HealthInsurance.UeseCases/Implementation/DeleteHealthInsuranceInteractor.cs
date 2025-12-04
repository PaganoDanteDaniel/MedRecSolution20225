using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class DeleteHealthInsuranceInteractor(
    IHealthInsuranceCommandRepository commandRepository,
    IHealthInsuranceQueriesRepository queriesRepository,
    IDeleteHealthInsuranceOutputPort presenter,
    IRepositoryUnitOfWork unitOfWork) : IDeleteHealthInsuranceInputPort
{
    public async Task Handle(Guid Id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            await unitOfWork.ExecuteWithRetry(async () =>
            {
                var entity = await queriesRepository.GetById(Id, ct);
                if (entity != null)
                {
                    try
                    {
                        await presenter.ErrorAsync(null);

                        entity.IsDeleted = true;

                        await unitOfWork.BeginTransaction();
                        await commandRepository.SoftDelete(entity, ct);
                        await unitOfWork.SaveChanges();
                        await unitOfWork.CommitTransaction();
                        await presenter.Handle(entity, ct);
                    }
                    catch (Exception)
                    {
                        await unitOfWork.RollbackTransaction();
                        throw;
                    }
                }
                else
                {
                    await presenter.ErrorAsync(
                        new ErrorInfo("EL REGISTO NO EXISTE O YA FUE ELIMINADO.", ErrorCode.NotFound));
                }

            }, ct);
        }
        catch (ConcurrencyException cx)
        {
            // 409: incluir conflictos tipados (lista de ConcurrencyConflictDto)
            await presenter.ErrorAsync(new ErrorInfo(
                "Conflicto de concurrencia: El registro fue modificado por otro usuario.",
                ErrorCode.ConcurrencyError,
                cx.Conflicts,
                409));
        }
        catch (UpdateException ux)
        {
            // 500: otros errores de persistencia
            await presenter.ErrorAsync(new ErrorInfo(
                "Error al persistir los cambios en la base de datos.",
                ErrorCode.UpdateError,
                ux.Details,
                500));
        }
        catch (BusinessException bx)
        {
            // Mantener compatibilidad con BusinessException si aparece desde otras capas
            await presenter.ErrorAsync(bx.Error);
        }
        catch (OperationCanceledException)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Operación cancelada por el usuario.",
                ErrorCode.Cancelled,
                null,
                499));
        }
        catch (Exception ex)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Ocurrió un error inesperado al eliminar el registro.",
                ErrorCode.DatabaseError,
                new { Exception = ex.Message },
                500));
        }
    }
}
