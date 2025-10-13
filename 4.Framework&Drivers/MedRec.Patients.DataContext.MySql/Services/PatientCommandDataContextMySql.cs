using MedRec.DataContext.MySql.Guard;
using MedRec.DataContext.MySql.Options;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.DataContext.MySql.DataContext;
using MedRec.Patients.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MedRec.Patients.DataContext.MySql.Services;
internal class PatientCommandDataContextMySql(IOptions<DBOptionsMySql> options)
    : PatientDataContext(options), IPatientCommandsDataContext
{
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
              await Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await Database.CommitTransactionAsync(cancellationToken);

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        Database.RollbackTransaction();
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await GuardDBContext.AgainstSaveChangesErrorAsync(this, cancellationToken);

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await operation();
        });
    }

    public async Task CreatePatientAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        patient.RowVersion = Guid.NewGuid().ToByteArray();
        await Patients.AddAsync(patient);
    }


    public async Task UpdatePatientAsync(Patient editPatient, CancellationToken cancellationToken = default)
    {
        var existingPatient = await Patients
            .FirstOrDefaultAsync(p => p.Id == editPatient.Id, cancellationToken);

        if (existingPatient == null)
            throw new InvalidOperationException("Paciente no encontrado.");

        Entry(existingPatient).CurrentValues.SetValues(editPatient);

        // Indicar el valor original de RowVersion para concurrencia
        Entry(existingPatient).OriginalValues["RowVersion"] = editPatient.RowVersion;

    }

    public async Task SoftDeletePatientAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await UpdatePatientAsync(patient, cancellationToken);
    }
    public async Task HardDeletePatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var value = await Patients.FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
        if (value != null) { Patients.Remove(value); }
    }
}
