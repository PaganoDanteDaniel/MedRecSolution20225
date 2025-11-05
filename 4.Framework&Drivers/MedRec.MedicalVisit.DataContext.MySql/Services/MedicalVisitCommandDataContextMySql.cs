using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Guard;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.MedicalVisit.DataContext.MySql.Services;
internal class MedicalVisitCommandDataContextMySql(DataBaseContextMySql context) :
   IMedicalVisitCommandDataContext
{

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        await context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await context.Database.CommitTransactionAsync(cancellationToken);
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
        await context.Database.RollbackTransactionAsync(cancellationToken);
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await GuardDBContext.AgainstSaveChangesErrorAsync(context.SaveChangesAsync, cancellationToken);
    public async Task CreateAsync(PatientMedicalVisit medicalVisit, CancellationToken cts = default) =>
        await context.PatientMedicalVisits.AddAsync(medicalVisit, cts);

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await operation();
        });
    }

    public async Task CreateMedicalHistoryAsync(PatientMedicalHistory medHist, CancellationToken cts = default) =>
        await context.PatientMedicalHistories.AddAsync(medHist, cts);

    public async Task UpdateAsync(PatientMedicalVisit medicalVisit, CancellationToken cts = default)
    {

        var existingVisit = await context.PatientMedicalVisits
            .FirstOrDefaultAsync(p => p.Id == medicalVisit.Id, cts);

        if (existingVisit == null)
            throw new InvalidOperationException("Paciente no encontrado.");

        context.Entry(existingVisit).CurrentValues.SetValues(medicalVisit);

        // Indicar el valor original de RowVersion para concurrencia
        context.Entry(existingVisit).OriginalValues["RowVersion"] = medicalVisit.RowVersion;

    }


}
