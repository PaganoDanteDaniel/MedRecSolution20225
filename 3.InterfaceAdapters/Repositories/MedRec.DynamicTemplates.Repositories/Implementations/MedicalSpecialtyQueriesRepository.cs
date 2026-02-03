using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Implementations;

internal class MedicalSpecialtyQueriesRepository(IMedicalSpecialtyQueriesDataContext queriesDataContext)
    : IMedicalSpecialtyQueriesRepositoryUoW
{
    public async Task<IEnumerable<MedicalSpecialty>> GetActiveSpecialties(CancellationToken cts = default) =>
        await queriesDataContext.GetActiveSpecialtiesAsync(cts);

    public async Task<MedicalSpecialty?> GetById(Guid id, CancellationToken cts = default) =>
        await queriesDataContext.GetByIdAsync(id, cts);

    public async Task<MedicalSpecialty?> GetByName(string name, CancellationToken cts = default) =>
        await queriesDataContext.GetByNameAsync(name, cts);
}