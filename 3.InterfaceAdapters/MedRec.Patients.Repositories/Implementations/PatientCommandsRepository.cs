using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.Repositories.Interfaces;
using MedRec.Patients.Repositories.Resources;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace MedRec.Patients.Repositories.Implementations;
internal class PatientCommandsRepository(
        IPatientCommandsDataContext commandsDb,
        IPatientQueriesDataContext queriesDb) : IPatientCommandsRepository
{
    private readonly IPatientCommandsDataContext _commandsDb = commandsDb;
    private readonly IPatientQueriesDataContext _queriesDb = queriesDb;


    // ----------------------------
    // Crear paciente
    // ----------------------------
    public async Task<Result<Patient>> Create(Patient patient, CancellationToken cancellationToken = default)
    {
        return await ExecuteTransactionAsync(async () =>
        {
            if (await _queriesDb.ExistsAsync(patient.DocumentNumber, cancellationToken))
            {
                throw new BusinessException(string.Format(
                        MessagesPatientsRepositories.ErrorAddingPatient,
                        patient.DocumentNumber),
                        ErrorCode.DuplicateKey);
            }
            await _commandsDb.CreatePatientAsync(patient, cancellationToken);
            return patient;
        }, cancellationToken);
    }

    // ----------------------------
    // Actualizar paciente
    // ----------------------------
    public async Task<Result<bool>> Update(Patient patient, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await ExecuteTransactionAsync(async () =>
        {
            if (!await _queriesDb.ExistsAsync(patient.Id, cts))
                throw new BusinessException(
                    MessagesPatientsRepositories.ErrorUpdatingPatient,
                    ErrorCode.NotFound);

            await _commandsDb.UpdatePatientAsync(patient, cts);
            return true;
        }, cts);
    }

    // ----------------------------
    // Hard Delete
    // ----------------------------
    public async Task<Result<bool>> HardDelete(Guid patientId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await ExecuteTransactionAsync(async () =>
                {
                    if (!await _queriesDb.ExistsAsync(patientId, cts))
                        throw new Exception(MessagesPatientsRepositories.ErrorDeletingPatient);

                    await _commandsDb.HardDeletePatientAsync(patientId, cts);
                    return true;
                }, cts);


    }

    // ----------------------------
    // Soft Delete
    // ----------------------------
    public async Task<Result<bool>> SoftDelete(Patient patient, CancellationToken cancellationToken = default)
    {

        return await ExecuteTransactionAsync(async () =>
        {
            patient.IsDeleted = true;
            await _commandsDb.SoftDeletePatientAsync(patient, cancellationToken);
            return true;
        }, cancellationToken);

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
            int rowaffected = 0;

            await _commandsDb.ExecuteWithRetryAsync(async () =>
            {
                await _commandsDb.BeginTransactionAsync(cancellationToken);

                value = await operation();

                rowaffected = await _commandsDb.SaveChangesAsync(cancellationToken);
                await _commandsDb.CommitTransactionAsync(cancellationToken);
            }, cancellationToken);

            return Result<T>.Ok(value, rowaffected);
        }
        catch (DuplicateKeyException dkey)
        {
            await SafeRollbackAsync(cancellationToken);
            // Extraer valor duplicado del mensaje
            var message = dkey.ToString();
            if (message.Contains("IX_Patients_DocumentNumber"))
            {
                return Result<T>.Fail(new ErrorInfo(
                                $"Ya existe un paciente con el mismo DNI.",
                                ErrorCode.DuplicateKey,
                                dkey.Entities // opcional, log interno
                            ));
            }
            return Result<T>.Fail(new ErrorInfo(
                "El registro ya existe. Verifica los datos e intenta nuevamente.",
                ErrorCode.DuplicateKey,
                $"Detalle técnico: {dkey.Message}" // solo para log interno, no para UI
            ));

        }
        catch (ConcurrencyException cex)
        {
            await SafeRollbackAsync(cancellationToken);
            return Result<T>.Fail(new ErrorInfo(
                "Conflicto de concurrencia al actualizar el registro.",
                ErrorCode.ConcurrencyError,
                cex.Details // puedo loggear valores Original/Current
            ));
        }
        catch (UpdateException uex)
        {
            await SafeRollbackAsync(cancellationToken);
            return Result<T>.Fail(new ErrorInfo(
                "Error al actualizar la base de datos.",
                ErrorCode.UpdateError,
                uex.Entities
            ));
        }
        catch (BusinessException ex)
        {
            await SafeRollbackAsync(cancellationToken);
            return Result<T>.Fail(ex.Error);
        }
        catch (Exception ex)
        {
            await SafeRollbackAsync(cancellationToken);
            return Result<T>.Fail(new ErrorInfo(
                "Error inesperado al ejecutar la operación: " + ex.Message,
                ErrorCode.Unknown
            ));
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
    private async Task SafeRollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _commandsDb.RollbackTransactionAsync(cancellationToken);
        }
        catch
        {
            // swallow rollback errors, ya que no deberían romper el flujo
        }
    }
}
