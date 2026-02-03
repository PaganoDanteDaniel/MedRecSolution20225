using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Interfaces;
public interface IMedicalVisitDynamicFieldCommandsDataContext
{
    Task CreateAsync(MedicalVisitDynamicField field, CancellationToken cts = default);
    Task UpdateAsync(MedicalVisitDynamicField field, CancellationToken cts = default);
    Task CreateRangeAsync(IEnumerable<MedicalVisitDynamicField> fields, CancellationToken cts = default);
    Task UpdateRangeAsync(IEnumerable<MedicalVisitDynamicField> fields, CancellationToken cts = default);
    Task DeleteByVisitIdAsync(Guid visitId, CancellationToken cts = default);
}