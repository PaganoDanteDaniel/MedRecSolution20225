using MedRec.DataContext.MySql.DataContext;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class RoleLookupDataContextMySql(MedRecContext context) : IRoleLookupDataContext
{
    public async Task<IReadOnlyList<RoleSummaryDto>> ListActiveAsync(CancellationToken ct = default)
    {
        return await context.Roles
            .Where(r => !r.IsDeleted)
            .Select(r => new RoleSummaryDto(r.Id, r.Name))
            .ToListAsync(ct);
    }
}
