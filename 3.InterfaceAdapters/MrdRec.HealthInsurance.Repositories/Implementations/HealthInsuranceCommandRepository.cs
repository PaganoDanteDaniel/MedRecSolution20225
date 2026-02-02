using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MrdRec.HealthInsurance.Repositories.Implementations;
internal class HealthInsuranceCommandRepository(
    IHealthInsuranceCommandsDataContext commandsDb) : IHealthInsuranceCommandRepository
{

    public async Task Create(HealthInsuranceCompany entity, CancellationToken cts) =>
        await commandsDb.CreateAsync(entity, cts);

    public async Task Update(HealthInsuranceCompany entity, CancellationToken cts) =>
        await commandsDb.UpdateAsync(entity, cts);

    public async Task SoftDelete(HealthInsuranceCompany entity, CancellationToken cts) =>
            await Update(entity, cts);

    public async Task HardDelete(HealthInsuranceCompany entity, CancellationToken cts) =>
            await commandsDb.DeleteAsync(entity, cts);
}
