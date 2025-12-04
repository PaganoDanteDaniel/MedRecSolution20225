using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MedRec.HealthInsurance.DataContext.EF.Services;
internal class HealthInsuranceQueriesDataContext(MedRecContext context) :
    IHealthInsuranceQueriesDataContext
{
    public Task<bool> ExistAsync(Guid id, CancellationToken ct) =>
        context.HealthInsuranceCompanies
            .AsNoTracking()
            .AnyAsync(h => h.Id == id && !h.IsDeleted, ct);

    public async Task<IEnumerable<HealthInsuranceCompany>> GetAllAsync(PaginationDto paginationDto, CancellationToken cancellationToken)
    {
        var query = context.HealthInsuranceCompanies
            .AsNoTracking()
            .Where(h => !h.IsDeleted);

        if (!string.IsNullOrWhiteSpace(paginationDto.FilterOne))
        {
            var filter = paginationDto.FilterOne.ToLower();
            query = query.Where(h =>
                h.Name.ToLower().Contains(filter) ||
                h.Acronym.ToLower().Contains(filter));
        }

        var skip = (paginationDto.CurrentPage - 1) * paginationDto.PageSize;
        var result = await query
            .OrderBy(h => h.Name)
            .Skip(skip)
            .Take(paginationDto.PageSize)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<HealthInsuranceCompany> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.HealthInsuranceCompanies
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(string filter = null, CancellationToken cancellationToken = default)
    {
        var query = context.HealthInsuranceCompanies
            .AsNoTracking()
            .Where(h => !h.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var filterLower = filter.ToLower();
            query = query.Where(h =>
                h.Name.ToLower().Contains(filterLower) ||
                h.Acronym.ToLower().Contains(filterLower));
        }

        return await query.CountAsync(cancellationToken);
    }
}
