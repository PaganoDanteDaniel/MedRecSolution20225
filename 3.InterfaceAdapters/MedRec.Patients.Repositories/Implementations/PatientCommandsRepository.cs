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
            if (await _queriesDb.ExistsAsync(patient.Id, cts))
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
        catch (DuplicateKeyException dkex)
        {
            await SafeRollbackAsync(cancellationToken);
            // Extraer valor duplicado del mensaje
            var message = dkex.ToString();
            if (message.Contains("IX_Patients_DocumentNumber"))
            {
                return Result<T>.Fail(new ErrorInfo(
                                $"Ya existe un paciente con el mismo DNI.",
                                ErrorCode.DuplicateKey,
                                dkex.Entities // opcional, log interno
                            ));
            }
            return Result<T>.Fail(new ErrorInfo(
                "El registro ya existe. Verifica los datos e intenta nuevamente.",
                ErrorCode.DuplicateKey,
                $"Detalle técnico: {dkex.Message}" // solo para log interno, no para UI
            ));

        }
        catch (ConcurrencyException cex)
        {
            await SafeRollbackAsync(cancellationToken);
            return Result<T>.Fail(new ErrorInfo(
                "Conflicto de concurrencia al actualizar el registro.",
                ErrorCode.ConcurrencyError,
                cex.Details // podés loggear valores Original/Current
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
    private string GetFullExceptionMessage(Exception ex)
    {
        if (ex == null) return string.Empty;

        var messages = new List<string>();
        while (ex != null)
        {
            messages.Add(ex.Message);
            ex = ex.InnerException;
        }
        return string.Join(" ---> ", messages);
    }
}


//internal class PatientCommandsRepository(
//    IPatientCommandsDataContext commandsDataContext,
//    IPatientQueriesDataContext queriesDataContext) : IPatientCommandsRepository
//{
//    private readonly IPatientCommandsDataContext _commandsDataContext = commandsDataContext;
//    private readonly IPatientQueriesDataContext _queriesDataContext = queriesDataContext;
//    public async Task<Result<Patient>> Create(Patient patient, CancellationToken cts = default)
//    {
//        Result<Patient> result = null!; // variable para almacenar el resultado dentro de la transacción
//        try
//        {
//            await ExecuteTransactionAsync(async () =>
//                    {
//                        // Verificamos si ya existe el paciente
//                        if (await _queriesDataContext.ExistsAsync(patient.DocumentNumber, cts))
//                        {
//                            result = Result<Patient>.Fail(String.Format(
//                                MessagesPatientsRepositories.ErrorAddingPatient,
//                                patient.DocumentNumber));
//                            return; // salimos de la transacción
//                        }

//                        // Creamos el paciente
//                        await _commandsDataContext.CreatePatientAsync(patient, cts);

//                        // Asignamos el paciente recién creado como resultado
//                        result = Result<Patient>.Ok(patient);

//                    }, cts);

//            return result;
//        }
//        catch (InvalidOperationException ie)
//        {
//            return Result<Patient>.Fail(ie.ErrorMessage);
//        }
//        catch (Exception ex)
//        {
//            return Result<Patient>.Fail(ex.ErrorMessage);

//        }

//    }



//    public async Task HardDelete(Patient patient, CancellationToken cts = default)
//    {
//        await ExecuteTransactionAsync(async () =>
//        {
//            if (!await _queriesDataContext.ExistsAsync(patient.Id))
//            {
//                throw new Exception(MessagesPatientsRepositories.ErrorDeletingPatient);
//            }
//            await _commandsDataContext.HardDeletePatientAsync(patient, cts);
//        }, cts);
//    }

//    public async Task SoftDelete(Patient patient, CancellationToken cts = default)
//    {
//        await ExecuteTransactionAsync(async () =>
//        {
//            var existing = await _queriesDataContext.GetPatientByIdAsync(patient.Id, cts);
//            if (existing == null || existing.IsDeleted == true)
//            {
//                throw new Exception(MessagesPatientsRepositories.ErrorDeletingPatient);
//            }
//            patient.IsDeleted = true;
//            await _commandsDataContext.SoftDeletePatientAsync(patient, cts);
//        }, cts);
//    }

//    public async Task Update(Patient patient, CancellationToken cts = default)
//    {
//        await ExecuteTransactionAsync(async () =>
//        {
//            if (!await _queriesDataContext.ExistsAsync(patient.Id))
//            {
//                throw new Exception(MessagesPatientsRepositories.ErrorUpdatingPatient);
//            }
//            await _commandsDataContext.UpdatePatientAsync(patient, cts);
//        }, cts);
//    }

//    public async Task ExecuteTransactionAsync(Func<Task> operation, CancellationToken cts = default)
//    {
//        if (operation == null)
//            throw new ArgumentNullException(nameof(operation));

//        await _commandsDataContext.ExecuteWithRetryAsync(async () =>
//        {
//            try
//            {
//                await _commandsDataContext.BeginTransactionAsync(cts);
//            }
//            catch (Exception ex)
//            {
//                // Manejar error de conexión explícitamente
//                throw new Exception("No se pudo iniciar la transacción: " + ex.ErrorMessage, ex);
//            }
//            try
//            {
//                await operation(); // Ejecuta la operación (Create, Update, etc.)
//                await _commandsDataContext.SaveChangesAsync(cts); // Guarda cambios
//                await _commandsDataContext.CommitTransactionAsync(cts);
//            }
//            catch
//            {
//                try
//                {
//                    await _commandsDataContext.RollbackTransactionAsync(cts);
//                    throw;
//                }
//                catch (Exception)
//                {

//                    throw;
//                }

//            }
//        }, cts);
//    }
//}
