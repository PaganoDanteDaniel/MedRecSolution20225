using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Interfaces;
public interface ITemplateFieldDefinitionQueriesDataContext
{
    Task<IEnumerable<TemplateFieldDefinition>> GetBySpecialtyIdAsync(Guid specialtyId, CancellationToken cts = default);
    Task<TemplateFieldDefinition?> GetByIdAsync(Guid id, CancellationToken cts = default);
    Task<IEnumerable<TemplateFieldDefinition>> GetActiveBySpecialtyIdAsync(Guid specialtyId, CancellationToken cts = default);
}
