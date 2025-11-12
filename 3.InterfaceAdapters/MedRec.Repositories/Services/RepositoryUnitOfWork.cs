using MedRec.DataContext.MySql.Guard;
using MedRec.Entity.Interfaces;

namespace MedRec.Repositories.Services;
internal class RepositoryUnitOfWork(IDataContextUnitOfWork dataContextUnitOfWork) : IRepositoryUnitOfWork
{
    public async Task BeginTransaction(CancellationToken ct = default) =>
        await dataContextUnitOfWork.BeginTransactionAsync(ct);

    public async Task CommitTransaction(CancellationToken ct = default) =>
        await dataContextUnitOfWork.CommitTransactionAsync(ct);

    public async Task RollbackTransaction(CancellationToken ct = default) =>
        await dataContextUnitOfWork.RollbackTransactionAsync(ct);

    public Task<int> SaveChanges(CancellationToken ct = default) =>
        GuardDBContext.AgainstSaveChangesErrorAsync(dataContextUnitOfWork.SaveChangesAsync, ct);
    //dataContextUnitOfWork.SaveChangesAsync(ct);

    public Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken ct = default) =>
        dataContextUnitOfWork.ExecuteWithRetryAsync(operation, ct);

    public void Dispose() { }
}
