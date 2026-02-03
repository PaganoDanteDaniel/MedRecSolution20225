using MedRec.Entity.POCOEntities;

namespace MrdRec.HealthInsurance.Repositories.Interfaces;
public interface IHealthInsuranceCommandsDataContext
{
    Task CreateAsync(HealthInsuranceCompany healthCompany, CancellationToken cts);
    Task UpdateAsync(HealthInsuranceCompany healthCompany, CancellationToken cts);
    Task DeleteAsync(HealthInsuranceCompany healthCompany, CancellationToken cts);
}
