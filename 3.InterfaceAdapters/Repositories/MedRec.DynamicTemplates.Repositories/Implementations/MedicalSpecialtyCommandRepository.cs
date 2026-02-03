using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Implementations;

internal class MedicalSpecialtyCommandRepository(IMedicalSpecialtyCommandsDataContext commandsDataContext)
    : IMedicalSpecialtyCommandRepositoryUoW
{
    public async Task Create(MedicalSpecialty specialty, CancellationToken cts = default) =>
        await commandsDataContext.CreateAsync(specialty, cts);

    public async Task Update(MedicalSpecialty specialty, CancellationToken cts = default) =>
        await commandsDataContext.UpdateAsync(specialty, cts);

    public async Task Delete(Guid id, CancellationToken cts = default)
    {
        var specialty = new MedicalSpecialty { Id = id };
        await commandsDataContext.DeleteAsync(specialty, cts);
    }
}