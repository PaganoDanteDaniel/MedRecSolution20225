
using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Guard;
using MedRec.DataContext.MySql.Options;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MedRec.HealthInsurance.DataContext.EF.Services;
internal class HealthInsuranceCommandsDataContext(IOptions<DBOptionsMySql> options) :
    DataBaseContextMySql(options), IHealthInsuranceCommandsDataContext
{
    public async Task CreateAsync(HealthInsuranceCompany healthCompany, CancellationToken cancellationToken = default)
    {
        HealthInsuranceCompanies.Add(healthCompany);
    }

    public async Task UpdateAsync(HealthInsuranceCompany healthCompany, CancellationToken cancellationToken = default)
    {
        var entity = await HealthInsuranceCompanies.FindAsync(healthCompany.Id, cancellationToken);
        if (entity != null)
        {
            entity.Name = healthCompany.Name;
            entity.Acronym = healthCompany.Acronym;
            entity.RowVersion = healthCompany.RowVersion;
        }
    }

    public async Task DeleteAsync(HealthInsuranceCompany healthCompany, CancellationToken cancellationToken = default)
    {
        var entity = await HealthInsuranceCompanies.FindAsync(healthCompany.Id, cancellationToken);
        if (entity != null) { HealthInsuranceCompanies.Remove(entity); }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction == null)
            await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction != null)
            await Database.CurrentTransaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction != null)
            await Database.CurrentTransaction.RollbackAsync(cancellationToken);
    }

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await operation();
        });
    }

    // Usa el Guard para traducir errores de EF Core
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await GuardDBContext.AgainstSaveChangesErrorAsync(base.SaveChangesAsync, cancellationToken);
}