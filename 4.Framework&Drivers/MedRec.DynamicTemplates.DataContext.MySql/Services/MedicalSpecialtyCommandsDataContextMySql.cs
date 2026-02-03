using MedRec.DataContext.MySql.DataContext;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;

namespace MedRec.DynamicTemplates.DataContext.MySql.Services;

internal class MedicalSpecialtyCommandsDataContextMySql(MedRecContext context) :
    IMedicalSpecialtyCommandsDataContext
{
    public async Task CreateAsync(MedicalSpecialty specialty, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();
        await context.MedicalSpecialties.AddAsync(specialty, cts);
    }

    public async Task UpdateAsync(MedicalSpecialty specialty, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        // Desadjuntar cualquier entidad rastreada con el mismo Id
        var trackedEntry = context.ChangeTracker.Entries<MedicalSpecialty>()
            .FirstOrDefault(e => e.Entity.Id == specialty.Id);

        if (trackedEntry != null)
        {
            trackedEntry.State = EntityState.Detached;
        }

        context.MedicalSpecialties.Update(specialty);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(MedicalSpecialty specialty, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();
        context.MedicalSpecialties.Remove(specialty);
        await Task.CompletedTask;
    }
}