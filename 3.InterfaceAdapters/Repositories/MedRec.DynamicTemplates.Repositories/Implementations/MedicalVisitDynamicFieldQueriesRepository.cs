using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Implementations;

internal class MedicalVisitDynamicFieldQueriesRepository(IMedicalVisitDynamicFieldQueriesDataContext queriesDataContext)
    : IMedicalVisitDynamicFieldQueriesRepositoryUoW
{
    public async Task<IEnumerable<MedicalVisitDynamicField>> GetByVisitId(Guid visitId, CancellationToken cts = default) =>
        await queriesDataContext.GetByVisitIdAsync(visitId, cts);

    public async Task<MedicalVisitDynamicField?> GetByVisitAndFieldDefinition(Guid visitId, Guid fieldDefinitionId, CancellationToken cts = default) =>
        await queriesDataContext.GetByVisitAndFieldDefinitionAsync(visitId, fieldDefinitionId, cts);
}