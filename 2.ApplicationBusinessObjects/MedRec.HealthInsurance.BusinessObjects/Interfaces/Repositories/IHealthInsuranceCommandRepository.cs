using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
public interface IHealthInsuranceCommandRepository : ICommandUnitOfWork
{
    Task<Result<HealthInsuranceCompany>> Create(HealthInsuranceCompany entity, CancellationToken cts);
    Task<Result<Unit>> Update(HealthInsuranceCompany entity, CancellationToken cts);
    Task<Result<Unit>> HardDelete(HealthInsuranceCompany entity, CancellationToken cts);
    Task<Result<Unit>> SoftDelete(HealthInsuranceCompany entity, CancellationToken cts);
}
