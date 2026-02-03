using MedRec.DataContext.MySql.DataContext;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;

namespace MedRec.DynamicTemplates.DataContext.MySql.Services;

internal class MedicalVisitDynamicFieldCommandsDataContextMySql(MedRecContext context) :
    IMedicalVisitDynamicFieldCommandsDataContext
{
    public async Task CreateAsync(MedicalVisitDynamicField field, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();
        await context.MedicalVisitDynamicFields.AddAsync(field, cts);
    }

    public async Task UpdateAsync(MedicalVisitDynamicField field, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        var trackedEntry = context.ChangeTracker.Entries<MedicalVisitDynamicField>()
            .FirstOrDefault(e => e.Entity.Id == field.Id);

        if (trackedEntry != null)
        {
            trackedEntry.State = EntityState.Detached;
        }

        context.MedicalVisitDynamicFields.Update(field);
        await Task.CompletedTask;
    }

    public async Task CreateRangeAsync(IEnumerable<MedicalVisitDynamicField> fields, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();
        await context.MedicalVisitDynamicFields.AddRangeAsync(fields, cts);
    }

    public async Task UpdateRangeAsync(IEnumerable<MedicalVisitDynamicField> fields, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();
        context.MedicalVisitDynamicFields.UpdateRange(fields);
        await Task.CompletedTask;
    }

    public async Task DeleteByVisitIdAsync(Guid visitId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        var fields = await context.MedicalVisitDynamicFields
            .Where(f => f.PatientMedicalVisitId == visitId)
            .ToListAsync(cts);

        context.MedicalVisitDynamicFields.RemoveRange(fields);
    }
}