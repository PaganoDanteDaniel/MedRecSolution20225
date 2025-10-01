using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;

namespace MrdRec.HealthInsurance.Repositories.Interfaces;
public interface IHealthInsuranceQueriesDataContext
{
    Task<HealthInsuranceCompany> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<HealthInsuranceCompany>> GetAllAsync(PaginationDto paginationDto, CancellationToken cancellationToken);
    Task<int> GetTotalCountAsync(string filter = null, CancellationToken cancellationToken = default);
}
