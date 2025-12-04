using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.Repositories.Interfaces;
using MedRec.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MedRec.MedicalVisit.DataContext.MySql.Services;
internal class MedicalVisitCommandDataContextMySql(MedRecContext context) :
   IMedicalVisitCommandDataContext
{
    public async Task CreateAsync(PatientMedicalVisit medicalVisit, CancellationToken ct = default) =>
        await context.PatientMedicalVisits.AddAsync(medicalVisit, ct);
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
