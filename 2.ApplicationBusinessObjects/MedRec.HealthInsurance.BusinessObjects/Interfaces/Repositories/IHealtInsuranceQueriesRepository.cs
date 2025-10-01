using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
public interface IHealtInsuranceQueriesRepository
{
    Task<Result<IEnumerable<HealthInsuranceCompany>>> GetAll(PaginationDto paginationDto, CancellationToken cancellationToken);
    Task<Result<int>> GetCount(string filter, CancellationToken cancellationToken);
}
