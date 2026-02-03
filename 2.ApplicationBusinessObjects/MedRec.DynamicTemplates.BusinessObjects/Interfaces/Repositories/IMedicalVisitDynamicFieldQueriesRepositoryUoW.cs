using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;

public interface IMedicalVisitDynamicFieldQueriesRepositoryUoW
{
    Task<IEnumerable<MedicalVisitDynamicField>> GetByVisitId(Guid visitId, CancellationToken cts = default);
    Task<MedicalVisitDynamicField?> GetByVisitAndFieldDefinition(Guid visitId, Guid fieldDefinitionId, CancellationToken cts = default);
}