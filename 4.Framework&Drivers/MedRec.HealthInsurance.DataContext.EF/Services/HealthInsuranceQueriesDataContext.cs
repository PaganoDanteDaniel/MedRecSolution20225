using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Options;
using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MedRec.HealthInsurance.DataContext.EF.Services;
internal class HealthInsuranceQueriesDataContext(IOptions<DBOptionsMySql> options) :
    DataBaseContextMySql(options), IHealthInsuranceQueriesDataContext
{
    public async Task<IEnumerable<HealthInsuranceCompany>> GetAllAsync(PaginationDto paginationDto, CancellationToken cancellationToken)
    {
        var query = HealthInsuranceCompanies
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
        return await HealthInsuranceCompanies
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(string filter = null, CancellationToken cancellationToken = default)
    {
        var query = HealthInsuranceCompanies
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
