using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Implementations;

internal class TemplateFieldDefinitionCommandRepository(ITemplateFieldDefinitionCommandsDataContext commandsDataContext)
    : ITemplateFieldDefinitionCommandRepositoryUoW
{
    public async Task Create(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default) =>
        await commandsDataContext.CreateAsync(fieldDefinition, cts);

    public async Task Update(TemplateFieldDefinition fieldDefinition, CancellationToken cts = default) =>
        await commandsDataContext.UpdateAsync(fieldDefinition, cts);

    public async Task Delete(Guid id, CancellationToken cts = default)
    {
        var fieldDefinition = new TemplateFieldDefinition { Id = id };
        await commandsDataContext.DeleteAsync(fieldDefinition, cts);
    }

    public async Task CreateRange(IEnumerable<TemplateFieldDefinition> fieldDefinitions, CancellationToken cts = default) =>
        await commandsDataContext.CreateRangeAsync(fieldDefinitions, cts);
}