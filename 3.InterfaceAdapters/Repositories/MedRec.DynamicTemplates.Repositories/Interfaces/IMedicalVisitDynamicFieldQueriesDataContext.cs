using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Interfaces;

public interface IMedicalVisitDynamicFieldQueriesDataContext
{
    Task<IEnumerable<MedicalVisitDynamicField>> GetByVisitIdAsync(Guid visitId, CancellationToken cts = default);
    Task<MedicalVisitDynamicField?> GetByVisitAndFieldDefinitionAsync(Guid visitId, Guid fieldDefinitionId, CancellationToken cts = default);
}