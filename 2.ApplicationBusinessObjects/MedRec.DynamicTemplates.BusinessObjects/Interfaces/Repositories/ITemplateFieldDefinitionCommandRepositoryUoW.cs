using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;

public interface ITemplateFieldDefinitionCommandRepositoryUoW
{
    Task Create(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default);
    Task Update(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default);
    Task Delete(Guid id, CancellationToken cts = default);
    Task CreateRange(IEnumerable<TemplateFieldDefinition> fieldDefinitions, CancellationToken cts = default);
}