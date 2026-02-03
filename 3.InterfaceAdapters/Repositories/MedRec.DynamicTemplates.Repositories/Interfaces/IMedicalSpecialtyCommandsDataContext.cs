using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.Repositories.Interfaces;

public interface IMedicalSpecialtyCommandsDataContext
{
    Task CreateAsync(MedicalSpecialty specialty, CancellationToken cts = default);
    Task UpdateAsync(MedicalSpecialty specialty, CancellationToken cts = default);
    Task DeleteAsync(MedicalSpecialty specialty, CancellationToken cts = default);
}