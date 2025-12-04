using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Patients.DataContext.MySql.Services;
internal class PatientCommandDataContextMySql(MedRecContext context)
    : IPatientCommandsDataContext
{
    public async Task CreatePatientAsync(Patient patient, CancellationToken cancellationToken = default) =>
        await context.Patients.AddAsync(patient, cancellationToken);

    public async Task UpdatePatientAsync(Patient editPatient, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 1) Asegurar que no haya otra instancia del mismo agregado rastreada
        var tracked = context.ChangeTracker.Entries<Patient>()
            .FirstOrDefault(e => e.Entity.Id == editPatient.Id);
        if (tracked is not null)
        {
            tracked.State = EntityState.Detached;
        }

        // 2) Crear/adjuntar un "stub" y aplicar los cambios del DTO
        //    Usamos la instancia recibida (no-tracked) como base.
        context.Attach(editPatient);

        var entry = context.Entry(editPatient);

        // 3) Marcar como Modified para actualización completa (o setear por propiedad si necesitas parcial)
        entry.State = EntityState.Modified;

        // 4) Establecer el valor ORIGINAL del token de concurrencia (RowVersion)
        //    Debe ser el valor que llegó del cliente (editPatient.RowVersion),
        //    no el de la BD. EF comparará UserValue vs BD en SaveChanges.
        entry.Property(nameof(Patient.RowVersion)).OriginalValue = editPatient.RowVersion;

        // NOTA: No llamamos SaveChanges aquí. El UoW lo hará y
        // GuardDBContext interceptará y clasificará las excepciones (incluida la concurrencia).
        await Task.CompletedTask;
    }

    public async Task SoftDeletePatientAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await UpdatePatientAsync(patient, cancellationToken);
    }
    public async Task HardDeletePatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var trackedEntry = context.ChangeTracker.Entries<Patient>()
            .FirstOrDefault(e => e.Entity.Id == patientId);

        if (trackedEntry != null)
            trackedEntry.State = EntityState.Detached;


        var value = await context.Patients.FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
        if (value != null) { context.Patients.Remove(value); }
    }
}
