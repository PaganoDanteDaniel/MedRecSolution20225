using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.Repositories.Interfaces;

namespace MedRec.Professionals.Repositories.Implementations;
internal class ProfessionalRepository(IProfessionalDataContext dataContext) : IProfessionalRepositoryUoW
{
    public Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct = default) => dataContext.GetByIdAsync(id, ct);
    public Task<Professional?> GetByEmailAsync(string email, CancellationToken ct = default) => dataContext.GetByEmailAsync(email, ct);
    public Task<IReadOnlyList<ProfessionalDto>> ListAsync(ProfessionalType? typeFilter, CancellationToken ct = default) => dataContext.ListAsync(typeFilter, ct);
    public Task CreateAsync(Professional professional, CancellationToken ct = default) => dataContext.CreateAsync(professional, ct);
    public Task UpdateAsync(Professional professional, CancellationToken ct = default) => dataContext.UpdateAsync(professional, ct);
    public Task SoftDeleteAsync(Guid id, CancellationToken ct = default) => dataContext.SoftDeleteAsync(id, ct);
}
