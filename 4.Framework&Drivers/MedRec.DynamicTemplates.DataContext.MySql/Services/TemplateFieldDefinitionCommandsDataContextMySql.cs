using MedRec.DataContext.MySql.DataContext;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;

namespace MedRec.DynamicTemplates.DataContext.MySql.Services;

internal class TemplateFieldDefinitionCommandsDataContextMySql(MedRecContext context) :
    ITemplateFieldDefinitionCommandsDataContext
{
    public async Task CreateAsync(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();
        await context.TemplateFieldDefinitions.AddAsync(fieldDefinition, cts);
    }

    public async Task UpdateAsync(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        var trackedEntry = context.ChangeTracker.Entries<TemplateFieldDefinition>()
            .FirstOrDefault(e => e.Entity.Id == fieldDefinition.Id);

        if (trackedEntry != null)
        {
            trackedEntry.State = EntityState.Detached;
        }

        context.TemplateFieldDefinitions.Update(fieldDefinition);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();
        context.TemplateFieldDefinitions.Remove(fieldDefinition);
        await Task.CompletedTask;
    }

    public async Task CreateRangeAsync(IEnumerable<TemplateFieldDefinition> fieldDefinitions, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();
        await context.TemplateFieldDefinitions.AddRangeAsync(fieldDefinitions, cts);
    }
}