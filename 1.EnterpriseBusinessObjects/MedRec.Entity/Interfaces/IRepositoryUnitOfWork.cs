namespace MedRec.Entity.Interfaces;
public interface IRepositoryUnitOfWork : IDisposable
{
    Task BeginTransaction(CancellationToken ct = default);
    Task CommitTransaction(CancellationToken ct = default);
    Task RollbackTransaction(CancellationToken ct = default);
    Task<int> SaveChanges(CancellationToken ct = default);
    Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken ct = default);
}
