using MedRec.DataContext.MySql.DataContext;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Professionals.DataContext.MySql.Services;

internal class SpecialtyLookupDataContextMySql(MedRecContext context) : ISpecialtyLookupDataContext
{
    public async Task<IReadOnlyList<SpecialtySummaryDto>> ListActiveAsync(CancellationToken ct = default) =>
        await context.MedicalSpecialties
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .Select(s => new SpecialtySummaryDto(s.Id, s.Name))
            .ToListAsync(ct);
}
