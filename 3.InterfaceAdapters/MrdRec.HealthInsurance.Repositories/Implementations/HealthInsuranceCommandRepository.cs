using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MrdRec.HealthInsurance.Repositories.Implementations;
internal class HealthInsuranceCommandRepository(IHealthInsuranceCommandsDataContext commandsDb,
        IHealthInsuranceQueriesDataContext queriesDb) :
    IHealthInsuranceCommandRepository
{
    private readonly IHealthInsuranceCommandsDataContext _commandsDb = commandsDb;
    private readonly IHealthInsuranceQueriesDataContext _queriesDb = queriesDb;

    public async Task<Result<HealthInsuranceCompany>> Create(HealthInsuranceCompany entity, CancellationToken cts)
    {
        try
        {
            cts.ThrowIfCancellationRequested();
            return await ExecuteTransactionAsync(async () =>
            {
                await _commandsDb.CreateAsync(entity, cts);
                return entity;
            }, cts);
        }
        catch (OperationCanceledException)
        {
            return Result<HealthInsuranceCompany>.Fail(new ErrorInfo("La operación fue cancelada.", ErrorCode.Unknown));
        }
        catch (ConcurrencyConflictException ex)
        {
            return Result<HealthInsuranceCompany>.Fail(new ErrorInfo(ex.Message, ErrorCode.ConcurrencyError));
        }
        catch (Exception ex)
        {
            return Result<HealthInsuranceCompany>.Fail(new ErrorInfo("Error al crear la entidad: " + ex.Message, ErrorCode.Unknown));
        }
    }

    public async Task<Result<Unit>> Update(HealthInsuranceCompany entity, CancellationToken cts)
    {
        try
        {
            cts.ThrowIfCancellationRequested();
            return await ExecuteTransactionAsync(async () =>
            {
                await _commandsDb.UpdateAsync(entity, cts);
            }, cts);
        }
        catch (OperationCanceledException)
        {
            return Result<Unit>.Fail(new ErrorInfo("La operación fue cancelada.", ErrorCode.Unknown));
        }
        catch (ConcurrencyConflictException ex)
        {
            return Result<Unit>.Fail(new ErrorInfo(ex.Message, ErrorCode.ConcurrencyError));
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail(new ErrorInfo("Error al actualizar la entidad: " + ex.Message, ErrorCode.Unknown));
        }
    }

    public async Task<Result<Unit>> SoftDelete(HealthInsuranceCompany entity, CancellationToken cts)
    {
        try
        {
            cts.ThrowIfCancellationRequested();
            return await ExecuteTransactionAsync(async () =>
            {
                var existing = await _queriesDb.GetByIdAsync(entity.Id, cts);
                if (existing != null)
                {
                    existing.IsDeleted = true;
                    await Update(existing, cts);
                }
                else
                {
                    throw new Exception("El registro no existe o ya fue eliminado con anterioridad.");
                }


            }, cts);
        }
        catch (OperationCanceledException)
        {
            return Result<Unit>.Fail(new ErrorInfo("La operación fue cancelada.", ErrorCode.Unknown));
        }
        catch (ConcurrencyConflictException ex)
        {
            return Result<Unit>.Fail(new ErrorInfo(ex.Message, ErrorCode.ConcurrencyError));
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail(new ErrorInfo("Error al eliminar la entidad: " + ex.Message, ErrorCode.Unknown));
        }
    }

    public async Task<Result<Unit>> HardDelete(HealthInsuranceCompany entity, CancellationToken cts)
    {
        try
        {
            cts.ThrowIfCancellationRequested();
            return await ExecuteTransactionAsync(async () =>
            {
                var existing = await _queriesDb.GetByIdAsync(entity.Id, cts);

                if (existing != null)
                    await _commandsDb.DeleteAsync(existing, cts);
            }, cts);
        }
        catch (OperationCanceledException)
        {
            return Result<Unit>.Fail(new ErrorInfo("La operación fue cancelada.", ErrorCode.Unknown));
        }
        catch (ConcurrencyConflictException ex)
        {
            return Result<Unit>.Fail(new ErrorInfo(ex.Message, ErrorCode.ConcurrencyError));
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail(new ErrorInfo("Error al eliminar la entidad: " + ex.Message, ErrorCode.Unknown));
        }
    }

    // ----------------------------
    // Ejecutar transacción genérica con valor
    // ----------------------------
    public async Task<Result<T>> ExecuteTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            T value = default!;

            await _commandsDb.ExecuteWithRetryAsync(async () =>
            {
                await _commandsDb.BeginTransactionAsync(cancellationToken);

                value = await operation();

                await _commandsDb.SaveChangesAsync(cancellationToken);
                await _commandsDb.CommitTransactionAsync(cancellationToken);
            }, cancellationToken);

            return Result<T>.Ok(value);
        }
        catch (Exception ex)
        {
            try { await _commandsDb.RollbackTransactionAsync(cancellationToken); } catch { }
            return Result<T>.Fail(new ErrorInfo("Error al ejecutar la operación: " + ex.Message, ErrorCode.Unknown));
        }
    }

    // ----------------------------
    // Ejecutar transacción genérica sin valor
    // ----------------------------
    public async Task<Result<Unit>> ExecuteTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteTransactionAsync(async () =>
        {
            await operation();
            return new Unit();
        }, cancellationToken);
    }
}
