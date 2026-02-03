using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;

namespace MrdRec.HealthInsurance.Repositories.Interfaces;
public interface IHealthInsuranceQueriesDataContext
{
    Task<HealthInsuranceCompany> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<HealthInsuranceCompany>> GetAllAsync(PaginationDto paginationDto, CancellationToken ct);
    Task<bool> ExistAsync(Guid id, CancellationToken ct);
    Task<int> GetTotalCountAsync(string filter = null, CancellationToken ct = default);
}
