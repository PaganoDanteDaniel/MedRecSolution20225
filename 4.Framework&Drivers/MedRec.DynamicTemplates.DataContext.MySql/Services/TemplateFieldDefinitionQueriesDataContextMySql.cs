using MedRec.DataContext.MySql.DataContext;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;

namespace MedRec.DynamicTemplates.DataContext.MySql.Services;

internal class TemplateFieldDefinitionQueriesDataContextMySql(MedRecContext context) :
    ITemplateFieldDefinitionQueriesDataContext
{
    public async Task<IEnumerable<TemplateFieldDefinition>> GetBySpecialtyIdAsync(Guid specialtyId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await context.TemplateFieldDefinitions
            .AsNoTracking()
            .Where(f => f.SpecialtyId == specialtyId)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(cts);
    }

    public async Task<TemplateFieldDefinition?> GetByIdAsync(Guid id, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await context.TemplateFieldDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cts);
    }

    public async Task<IEnumerable<TemplateFieldDefinition>> GetActiveBySpecialtyIdAsync(Guid specialtyId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await context.TemplateFieldDefinitions
            .AsNoTracking()
            .Where(f => f.SpecialtyId == specialtyId && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(cts);
    }
}