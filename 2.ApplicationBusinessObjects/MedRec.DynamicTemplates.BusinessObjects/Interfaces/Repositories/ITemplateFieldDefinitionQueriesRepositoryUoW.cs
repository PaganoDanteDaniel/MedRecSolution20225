using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;

public interface ITemplateFieldDefinitionQueriesRepositoryUoW
{
    Task<IEnumerable<TemplateFieldDefinition>> GetBySpecialtyId(Guid specialtyId, CancellationToken cts = default);
    Task<TemplateFieldDefinition?> GetById(Guid id, CancellationToken cts = default);
    Task<IEnumerable<TemplateFieldDefinition>> GetActiveBySpecialtyId(Guid specialtyId, CancellationToken cts = default);
}