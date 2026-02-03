using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Interfaces;

public interface ITemplateFieldDefinitionCommandsDataContext
{
    Task CreateAsync(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default);
    Task UpdateAsync(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default);
    Task DeleteAsync(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default);
    Task CreateRangeAsync(IEnumerable<TemplateFieldDefinition> fieldDefinitions, CancellationToken cts = default);
}