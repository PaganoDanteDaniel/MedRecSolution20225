using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;

public interface IMedicalSpecialtyCommandRepositoryUoW
{
    Task Create(MedicalSpecialty specialty, CancellationToken cts = default);
    Task Update(MedicalSpecialty specialty, CancellationToken cts = default);
    Task Delete(Guid id, CancellationToken cts = default);
}