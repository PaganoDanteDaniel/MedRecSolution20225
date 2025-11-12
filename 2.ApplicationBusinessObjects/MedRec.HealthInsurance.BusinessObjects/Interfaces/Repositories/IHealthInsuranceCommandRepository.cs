using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
public interface IHealthInsuranceCommandRepository : IUnitOfWork
{
    Task Create(HealthInsuranceCompany entity, CancellationToken cts);
    Task Update(HealthInsuranceCompany entity, CancellationToken cts);
    Task HardDelete(HealthInsuranceCompany entity, CancellationToken cts);
    Task SoftDelete(HealthInsuranceCompany entity, CancellationToken cts);
}
