using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;

public interface IMedicalSpecialtyQueriesRepositoryUoW
{
    Task<IEnumerable<MedicalSpecialty>> GetActiveSpecialties(CancellationToken cts = default);
    Task<MedicalSpecialty?> GetById(Guid id, CancellationToken cts = default);
    Task<MedicalSpecialty?> GetByName(string name, CancellationToken cts = default);
}