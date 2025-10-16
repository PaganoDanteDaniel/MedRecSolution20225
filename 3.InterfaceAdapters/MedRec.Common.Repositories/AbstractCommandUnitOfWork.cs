namespace MedRec.Common.Repositories;

using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.Results;
using MedRec.Shared.Exceptions.SQLExceptions;
using System.Threading;

public abstract class AbstractCommandUnitOfWork<TContext>
    where TContext : IDataContextUnitOfWork
{
    protected readonly TContext _commandsDb;

    protected AbstractCommandUnitOfWork(TContext commandsDb)
    {
        _commandsDb = commandsDb;
    }

    public virtual async Task<Result<T>> ExecuteTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            T value = default!;
            int rowAffected = 0;

            await _commandsDb.ExecuteWithRetryAsync(async () =>
            {
                await _commandsDb.BeginTransactionAsync(cancellationToken);

                value = await operation();

                rowAffected = await _commandsDb.SaveChangesAsync(cancellationToken);
                await _commandsDb.CommitTransactionAsync(cancellationToken);
            }, cancellationToken);

            return Result<T>.Ok(value, rowAffected);
        }
        catch (Exception ex)
        {
            await SafeRollbackAsync(cancellationToken);
            return HandleException<T>(ex);
        }
    }

    public virtual async Task<Result<Unit>> ExecuteTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteTransactionAsync(async () =>
        {
            await operation();
            return new Unit();
        }, cancellationToken);
    }

    protected virtual async Task SafeRollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _commandsDb.RollbackTransactionAsync(cancellationToken);
        }
        catch
        {
            // No romper el flujo por errores de rollback
        }
    }

    protected virtual Result<T> HandleException<T>(Exception ex)
    {
        if (ex is UpdateException uex)
        {
            return Result<T>.Fail(new ErrorInfo(
                "Error al actualizar la base de datos.",
                ErrorCode.UpdateError,
                uex.Entities
            ));
        }
        if (ex is ConcurrencyException cex)
        {
            return Result<T>.Fail(new ErrorInfo(
                "Conflicto de concurrencia al actualizar el registro.",
                ErrorCode.ConcurrencyError,
                cex.Details
            ));
        }
        return Result<T>.Fail(new ErrorInfo(
            "Error inesperado al ejecutar la operación: " + ex.Message,
            ErrorCode.Unknown
        ));
    }
}
