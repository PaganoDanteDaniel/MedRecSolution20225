using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Interfaces;

public interface IMedicalSpecialtyQueriesDataContext
{
    Task<IEnumerable<MedicalSpecialty>> GetActiveSpecialtiesAsync(CancellationToken cts = default);
    Task<MedicalSpecialty?> GetByIdAsync(Guid id, CancellationToken cts = default);
    Task<MedicalSpecialty?> GetByNameAsync(string name, CancellationToken cts = default);
}