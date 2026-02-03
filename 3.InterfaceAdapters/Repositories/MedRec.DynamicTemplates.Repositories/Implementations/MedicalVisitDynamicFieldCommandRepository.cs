using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Implementations;

internal class MedicalVisitDynamicFieldCommandRepository(IMedicalVisitDynamicFieldCommandsDataContext commandsDataContext)
    : IMedicalVisitDynamicFieldCommandRepositoryUoW
{
    public async Task Create(MedicalVisitDynamicField field, CancellationToken cts = default) =>
        await commandsDataContext.CreateAsync(field, cts);

    public async Task Update(MedicalVisitDynamicField field, CancellationToken cts = default) =>
        await commandsDataContext.UpdateAsync(field, cts);

    public async Task CreateRange(IEnumerable<MedicalVisitDynamicField> fields, CancellationToken cts = default) =>
        await commandsDataContext.CreateRangeAsync(fields, cts);

    public async Task UpdateRange(IEnumerable<MedicalVisitDynamicField> fields, CancellationToken cts = default) =>
        await commandsDataContext.UpdateRangeAsync(fields, cts);

    public async Task DeleteByVisitId(Guid visitId, CancellationToken cts = default) =>
        await commandsDataContext.DeleteByVisitIdAsync(visitId, cts);
}