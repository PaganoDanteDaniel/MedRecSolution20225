using MedRec.DataContext.MySql.DataContext;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class DoctorLookupDataContextMySql(MedRecContext context) : IDoctorLookupDataContext
{
    public async Task<IReadOnlyList<DoctorSummaryDto>> ListActiveAsync(CancellationToken ct = default)
    {
        return await context.Doctors
            .Where(d => !d.IsDeleted)
            .Select(d => new DoctorSummaryDto(d.Id, d.LastName + ", " + d.FirstName))
            .ToListAsync(ct);
    }
}
