using MedRec.DataContext.EF.Guard;
using MedRec.DataContext.EF.Options;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.DataContext.EF.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MedRec.HealthInsurance.DataContext.EF.Services;
internal class HealthInsuranceCommandsDataContext(IOptions<DBOptions> options) :
    HealthInsuranceContext(options), IHealthInsuranceCommandsDataContext
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
    async Task IDataContextUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await GuardDBContext.AgainstSaveChangesErrorAsync(this, cancellationToken);
    }
}

//public async Task CreateAsync(HealthInsuranceCompany healthCompany)
//{
//    HealthInsuranceCompanies.Add(healthCompany);
//}

//public async Task DeleteAsync(HealthInsuranceCompany healthCompany)
//{
//    var entity = await HealthInsuranceCompanies.FindAsync(healthCompany.Id);
//    if (entity != null)
//    {
//        entity.IsDeleted = true;
//    }
//}

//public async Task UpdateAsync(HealthInsuranceCompany healthCompany)
//{
//    var entity = await HealthInsuranceCompanies.FindAsync(healthCompany.Id);
//    if (entity != null)
//    {
//        entity.Name = healthCompany.Name;
//        entity.Acronym = healthCompany.Acronym;
//        entity.RowVersion = healthCompany.RowVersion;
//    }
//}
//public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
//{
//    if (Database.CurrentTransaction == null)
//        await Database.BeginTransactionAsync(cancellationToken);
//}

//public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
//{
//    if (Database.CurrentTransaction != null)
//        await Database.CurrentTransaction.CommitAsync(cancellationToken);
//}

//public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
//{
//    if (Database.CurrentTransaction != null)
//        await Database.CurrentTransaction.RollbackAsync(cancellationToken);
//}
//public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
//    {
//        var strategy = Database.CreateExecutionStrategy();
//        await strategy.ExecuteAsync(async () =>
//        {
//            const int maxRetries = 3;
//            int retryCount = 0;
//            while (true)
//            {
//                try
//                {
//                    await operation();
//                    break;
//                }
//                catch (DbUpdateConcurrencyException) when (retryCount < maxRetries)
//                {
//                    retryCount++;
//                    await Task.Delay(100 * retryCount, cancellationToken);
//                }
//            }
//        });
//    }
//public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
//{
//    try
//    {
//        await base.SaveChangesAsync(cancellationToken);
//    }
//    catch (DbUpdateConcurrencyException ex)
//    {
//        // Opcional: obtener la entidad actual desde la DB
//        var entry = ex.Entries.FirstOrDefault();
//        object? currentEntity = null;

//        if (entry != null)
//        {
//            var entityType = entry.Entity.GetType();
//            var key = entry.Properties.First(p => p.Metadata.IsPrimaryKey());
//            var id = key.CurrentValue;

//            currentEntity = await entry.Context
//                .Set(entityType)
//                .AsNoTracking()
//                .FirstOrDefaultAsync(e => EF.Property<object>(e, key.Metadata.Name) == id, cancellationToken);
//        }

//        throw new ConcurrencyConflictException(
//            "Conflicto de concurrencia al guardar cambios.",
//            currentEntity!
//        );
//    }
//}


////public async Task SaveChangesAsync(CancellationToken cancellationToken)
////{
////    try
////    {
////        await SaveChangesAsync();
////    }
////    catch (DbUpdateConcurrencyException)
////    {
////        // Recarga los datos actuales desde la base de datos
////        var currentEntity = await HealthInsuranceCompanies
////            .AsNoTracking()
////            .FirstOrDefaultAsync(e => e.Id == healthCompany.Id);

////        throw new ConcurrencyConflictException(
////            "Conflicto de concurrencia al actualizar la compañía de seguros.",
////            currentEntity
////        );
////    }
////    await base.SaveChangesAsync(cancellationToken);
////}