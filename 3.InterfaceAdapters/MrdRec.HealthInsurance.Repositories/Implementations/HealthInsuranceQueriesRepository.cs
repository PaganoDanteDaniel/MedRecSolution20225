using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MrdRec.HealthInsurance.Repositories.Implementations;
internal class HealthInsuranceQueriesRepository(IHealthInsuranceQueriesDataContext dataContext) : IHealthInsuranceQueriesRepository
{
    private readonly IHealthInsuranceQueriesDataContext _dataContext = dataContext;

    public async Task<HealthInsuranceCompany> GetById(Guid id, CancellationToken cancellationToken) =>
        await _dataContext.GetByIdAsync(id, cancellationToken);

    public async Task<IEnumerable<HealthInsuranceCompany>> GetAll(PaginationDto paginationDto, CancellationToken cancellationToken) =>
        await _dataContext.GetAllAsync(paginationDto, cancellationToken);


    public async Task<int> GetCount(string filter, CancellationToken cancellationToken) =>
        await _dataContext.GetTotalCountAsync(filter, cancellationToken);

    public async Task<bool> Exist(Guid id, CancellationToken ct) =>
        await _dataContext.ExistAsync(id, ct);
}
