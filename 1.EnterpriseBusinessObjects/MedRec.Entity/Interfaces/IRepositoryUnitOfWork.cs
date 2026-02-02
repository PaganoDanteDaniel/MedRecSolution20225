namespace MedRec.Entity.Interfaces;
public interface IRepositoryUnitOfWork : IDisposable
{
    Task BeginTransaction(CancellationToken ct = default);
    Task CommitTransaction(CancellationToken ct = default);
    Task RollbackTransaction(CancellationToken ct = default);
    Task<int> SaveChanges(CancellationToken ct = default);
    Task ExecuteWithRetry(Func<Task> operation, CancellationToken ct = default);
    Task ExecuteInTransactionWithRetry(Func<Task> work, CancellationToken ct = default);
}
