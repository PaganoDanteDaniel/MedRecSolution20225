using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
public interface IHealthInsuranceQueriesRepository
{
    Task<IEnumerable<HealthInsuranceCompany>> GetAll(PaginationDto paginationDto, CancellationToken ct);
    Task<HealthInsuranceCompany> GetById(Guid id, CancellationToken ct);
    Task<bool> Exist(Guid id, CancellationToken ct);
    Task<int> GetCount(string filter, CancellationToken ct);
}
