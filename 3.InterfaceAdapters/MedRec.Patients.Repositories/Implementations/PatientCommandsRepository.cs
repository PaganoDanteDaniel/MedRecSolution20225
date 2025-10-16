using MedRec.Common.Repositories;
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
        IPatientQueriesDataContext queriesDb) : AbstractCommandUnitOfWork<IPatientCommandsDataContext>(commandsDb),
    IPatientCommandsRepository
{

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
    protected override Result<T> HandleException<T>(Exception ex)
    {
        if (ex is DuplicateKeyException dkey)
        {
            var message = dkey.ToString();
            if (message.Contains("IX_Patients_DocumentNumber"))
            {
                return Result<T>.Fail(new ErrorInfo(
                    $"Ya existe un paciente con el mismo DNI.",
                    ErrorCode.DuplicateKey,
                    dkey.Entities
                ));
            }
            return Result<T>.Fail(new ErrorInfo(
                "El registro ya existe. Verifica los datos e intenta nuevamente.",
                ErrorCode.DuplicateKey,
                $"Detalle técnico: {dkey.Message}"
            ));
        }

        return base.HandleException<T>(ex);
    }

}
