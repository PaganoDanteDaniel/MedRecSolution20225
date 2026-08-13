using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.Repositories.Interfaces;
public interface IProfessionalDataContext
{
    Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Professional?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<ProfessionalDto>> ListAsync(ProfessionalType? typeFilter, CancellationToken ct = default);
    Task CreateAsync(Professional professional, CancellationToken ct = default);
    Task UpdateAsync(Professional professional, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
