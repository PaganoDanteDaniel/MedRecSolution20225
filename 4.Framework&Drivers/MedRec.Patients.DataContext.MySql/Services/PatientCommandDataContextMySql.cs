using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Guard;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Patients.DataContext.MySql.Services;
internal class PatientCommandDataContextMySql(DataBaseContextMySql context)
    : IPatientCommandsDataContext
{
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
              await context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await context.Database.CommitTransactionAsync(cancellationToken);

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        context.Database.RollbackTransaction();
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await GuardDBContext.AgainstSaveChangesErrorAsync(context.SaveChangesAsync, cancellationToken);

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await operation();
        });
    }

    public async Task CreatePatientAsync(Patient patient, CancellationToken cancellationToken = default) =>
        await context.Patients.AddAsync(patient, cancellationToken);


    public async Task UpdatePatientAsync(Patient editPatient, CancellationToken cancellationToken = default)
    {
        var existingPatient = await context.Patients
            .FirstOrDefaultAsync(p => p.Id == editPatient.Id, cancellationToken);

        if (existingPatient == null)
            throw new InvalidOperationException("Paciente no encontrado.");

        context.Entry(existingPatient).CurrentValues.SetValues(editPatient);

        // Indicar el valor original de RowVersion para concurrencia
        context.Entry(existingPatient).OriginalValues["RowVersion"] = editPatient.RowVersion;

    }

    public async Task SoftDeletePatientAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await UpdatePatientAsync(patient, cancellationToken);
    }
    public async Task HardDeletePatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var value = await context.Patients.FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
        if (value != null) { context.Patients.Remove(value); }
    }
}
