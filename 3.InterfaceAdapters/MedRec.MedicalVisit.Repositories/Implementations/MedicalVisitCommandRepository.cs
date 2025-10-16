using MedRec.Common.Repositories;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.Repositories.Interfaces;
using MedRec.Shared.Exceptions;

namespace MedRec.MedicalVisit.Repositories.Implementations;
internal class MedicalVisitCommandRepository(
    IMedicalVisitCommandDataContext commandsDb) :
    AbstractCommandUnitOfWork<IMedicalVisitCommandDataContext>(commandsDb),
    IMedicalVisitCommandRepository
{
    public async Task<Result<Unit>> Create(PatientMedicalVisit medicalVisit, CancellationToken cts = default)
    {
        try
        {
            cts.ThrowIfCancellationRequested();
            return await ExecuteTransactionAsync(async () =>
            {
                await _commandsDb.CreateAsync(medicalVisit, cts);

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
            return Result<Unit>.Fail(new ErrorInfo("Error al crear el registro: " + ex.Message, ErrorCode.Unknown));
        }
    }

    public async Task<Result<Guid>> CreateMedicalHistory(Guid patientId, CancellationToken cts = default)
    {
        try
        {
            cts.ThrowIfCancellationRequested();
            return await ExecuteTransactionAsync(async () =>
            {
                var medHist = new PatientMedicalHistory { PatientId = patientId };

                await _commandsDb.CreateMedicalHistoryAsync(medHist, cts);
                return medHist.Id;
            }, cts);
        }
        catch (OperationCanceledException)
        {
            return Result<Guid>.Fail(new ErrorInfo("La operación fue cancelada.", ErrorCode.Unknown));
        }
        catch (ConcurrencyConflictException ex)
        {
            return Result<Guid>.Fail(new ErrorInfo(ex.Message, ErrorCode.ConcurrencyError));
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail(new ErrorInfo("Error al crear el registro: " + ex.Message, ErrorCode.Unknown));
        }
    }
}
