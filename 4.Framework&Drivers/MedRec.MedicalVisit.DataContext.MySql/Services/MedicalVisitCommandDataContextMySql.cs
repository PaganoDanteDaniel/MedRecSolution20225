using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Guard;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.Repositories.Interfaces;
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

    public async Task UpdateAsync(PatientMedicalVisit medicalVisit, CancellationToken ct = default)
    {

        var existingVisit = await context.PatientMedicalVisits
            .FirstOrDefaultAsync(p => p.Id == medicalVisit.Id, ct);

        if (existingVisit == null)
            throw new InvalidOperationException("Paciente no encontrado.");

        context.Entry(existingVisit).CurrentValues.SetValues(medicalVisit);

        // Indicar el valor original de RowVersion para concurrencia
        context.Entry(existingVisit).OriginalValues["RowVersion"] = medicalVisit.RowVersion;

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
