using MedRec.DataContext.MySql.DataContext;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class ProfessionalLookupDataContextMySql(MedRecContext context) : IProfessionalLookupDataContext
{
    public async Task<IReadOnlyList<ProfessionalSummaryDto>> ListActiveAsync(CancellationToken ct = default)
    {
        return await context.Professionals
            .Where(p => !p.IsDeleted)
            .Select(p => new ProfessionalSummaryDto(p.Id, p.LastName + ", " + p.FirstName))
            .ToListAsync(ct);
    }
}
