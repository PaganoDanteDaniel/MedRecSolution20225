using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Professionals.DataContext.MySql.Services;

internal class ProfessionalDataContextMySql(MedRecContext context) : IProfessionalDataContext
{
    public async Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Professionals.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

    public async Task<Professional?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Professionals.FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted, ct);

    public async Task<IReadOnlyList<ProfessionalDto>> ListAsync(ProfessionalType? typeFilter, CancellationToken ct = default)
    {
        var query = context.Professionals.Where(p => !p.IsDeleted);
        if (typeFilter.HasValue)
            query = query.Where(p => p.Type == typeFilter.Value);

        return await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Select(p => new ProfessionalDto(p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.HireDate, p.Type, p.LicenseNumber, p.SpecialtyId, p.RowVersion))
            .ToListAsync(ct);
    }

    public Task CreateAsync(Professional professional, CancellationToken ct = default)
    {
        context.Professionals.Add(professional);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Professional professional, CancellationToken ct = default)
    {
        context.Professionals.Update(professional);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var professional = await context.Professionals.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (professional is not null)
            professional.IsDeleted = true;
    }
}
