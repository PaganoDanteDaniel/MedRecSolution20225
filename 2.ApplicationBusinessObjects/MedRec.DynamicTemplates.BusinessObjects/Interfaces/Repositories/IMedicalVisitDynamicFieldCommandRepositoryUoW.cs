using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;

public interface IMedicalVisitDynamicFieldCommandRepositoryUoW
{
    Task Create(MedicalVisitDynamicField field, CancellationToken cts = default);
    Task Update(MedicalVisitDynamicField field, CancellationToken cts = default);
    Task CreateRange(IEnumerable<MedicalVisitDynamicField> fields, CancellationToken cts = default);
    Task UpdateRange(IEnumerable<MedicalVisitDynamicField> fields, CancellationToken cts = default);
    Task DeleteByVisitId(Guid visitId, CancellationToken cts = default);
}