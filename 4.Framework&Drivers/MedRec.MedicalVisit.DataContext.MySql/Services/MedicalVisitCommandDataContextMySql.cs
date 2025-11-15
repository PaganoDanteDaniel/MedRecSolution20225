using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Guard;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.Repositories.Interfaces;
using MedRec.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MedRec.MedicalVisit.DataContext.MySql.Services;
internal class MedicalVisitCommandDataContextMySql(DataBaseContextMySql context) :
   IMedicalVisitCommandDataContext
{

    public async Task BeginTransactionAsync(CancellationToken ct = default) =>
        await context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default) =>
        await context.Database.CommitTransactionAsync(ct);
    public async Task RollbackTransactionAsync(CancellationToken ct = default) =>
        await context.Database.RollbackTransactionAsync(ct);
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await GuardDBContext.AgainstSaveChangesErrorAsync(context.SaveChangesAsync, ct);
    public async Task CreateAsync(PatientMedicalVisit medicalVisit, CancellationToken ct = default) =>
        await context.PatientMedicalVisits.AddAsync(medicalVisit, ct);

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await operation();
        });
    }

    public async Task CreateMedicalHistoryAsync(PatientMedicalHistory medHist, CancellationToken ct = default) =>
        await context.PatientMedicalHistories.AddAsync(medHist, ct);

    // Paso 1: Asegurar que no haya una entidad rastreada con el mismo Id
    //var trackedEntry = context.ChangeTracker.Entries<HealthInsuranceCompany>()
    //    .FirstOrDefault(e => e.Entity.Id == medicalVisit.Id);

    //if (trackedEntry != null)
    //{
    //    trackedEntry.State = EntityState.Detached;
    //}
    public async Task UpdateAsync(PatientMedicalVisit entity, CancellationToken ct = default)
    {
        try
        {
            // Paso 1: Asegurar que no haya una entidad rastreada con el mismo Id
            var trackedEntry = context.ChangeTracker.Entries<PatientMedicalVisit>()
                .FirstOrDefault(e => e.Entity.Id == entity.Id);

            if (trackedEntry != null)
            {
                trackedEntry.State = EntityState.Detached;
            }
            var existing = await context.PatientMedicalVisits
                .FirstOrDefaultAsync(h => h.Id == entity.Id, ct);
            if (existing is null)
                throw new BusinessException(new ErrorInfo("Obra Social no encontrada.", ErrorCode.NotFound, entity.Id, 404));

            context.Entry(existing).CurrentValues.SetValues(entity);
            context.Entry(existing).Property("RowVersion").OriginalValue = existing.RowVersion;

        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error al actualizar el registro en la base de datos.",
                              ErrorCode.UpdateError,
                              ex.InnerException?.Message ?? ex.Message, 500));
        }
        catch (Exception ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error inesperado en la capa de persistencia.",
                              ErrorCode.Unknown,
                              ex.Message, 500));
        }
    }

    public async Task<bool> HasMedicalHistoryAsync(Guid patientId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await context.PatientMedicalHistories
            .AsNoTracking()
            .AnyAsync(h => h.PatientId == patientId && !h.IsDeleted, ct);
    }

    public async Task<Guid> GetMedicalHistoryIdByPatientIdAsync(Guid patientId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var history = await context.PatientMedicalHistories
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.PatientId == patientId && !h.IsDeleted, ct);

        return history?.Id ?? Guid.Empty;
    }
}
