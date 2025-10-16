using MedRec.Common.Repositories;
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
    AbstractCommandUnitOfWork<IHealthInsuranceCommandsDataContext>(commandsDb), IHealthInsuranceCommandRepository
{
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
}
