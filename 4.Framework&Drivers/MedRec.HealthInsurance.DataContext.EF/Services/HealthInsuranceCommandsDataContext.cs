
using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Guard;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MedRec.HealthInsurance.DataContext.EF.Services;
internal class HealthInsuranceCommandsDataContext(DataBaseContextMySql context) :
    IHealthInsuranceCommandsDataContext
{
    public async Task CreateAsync(HealthInsuranceCompany healthCompany, CancellationToken cancellationToken = default) =>
        await context.HealthInsuranceCompanies.AddAsync(healthCompany);

    public async Task UpdateAsync(HealthInsuranceCompany healthCompany, CancellationToken cancellationToken = default)
    {
        var entity = await context.HealthInsuranceCompanies.FindAsync(healthCompany.Id, cancellationToken);
        if (entity != null)
        {
            entity.Name = healthCompany.Name;
            entity.Acronym = healthCompany.Acronym;
            entity.RowVersion = healthCompany.RowVersion;
        }
    }

    public async Task DeleteAsync(HealthInsuranceCompany healthCompany, CancellationToken cancellationToken = default)
    {
        var entity = await context.HealthInsuranceCompanies.FindAsync(healthCompany.Id, cancellationToken);
        if (entity != null) { context.HealthInsuranceCompanies.Remove(entity); }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (context.Database.CurrentTransaction == null)
            await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (context.Database.CurrentTransaction != null)
            await context.Database.CurrentTransaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (context.Database.CurrentTransaction != null)
            await context.Database.CurrentTransaction.RollbackAsync(cancellationToken);
    }

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await operation();
        });
    }

    // Usa el Guard para traducir errores de EF Core
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await GuardDBContext.AgainstSaveChangesErrorAsync(context.SaveChangesAsync, cancellationToken);
}