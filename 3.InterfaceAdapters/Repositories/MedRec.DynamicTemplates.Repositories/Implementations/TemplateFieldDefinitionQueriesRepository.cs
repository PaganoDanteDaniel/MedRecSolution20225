using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Implementations;

internal class TemplateFieldDefinitionQueriesRepository(ITemplateFieldDefinitionQueriesDataContext queriesDataContext)
    : ITemplateFieldDefinitionQueriesRepositoryUoW
{
    public async Task<IEnumerable<TemplateFieldDefinition>> GetBySpecialtyId(Guid specialtyId, CancellationToken cts = default) =>
        await queriesDataContext.GetBySpecialtyIdAsync(specialtyId, cts);

    public async Task<TemplateFieldDefinition?> GetById(Guid id, CancellationToken cts = default) =>
        await queriesDataContext.GetByIdAsync(id, cts);

    public async Task<IEnumerable<TemplateFieldDefinition>> GetActiveBySpecialtyId(Guid specialtyId, CancellationToken cts = default) =>
        await queriesDataContext.GetActiveBySpecialtyIdAsync(specialtyId, cts);
}