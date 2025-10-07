namespace MedRec.Entity.Interfaces;
public interface IDataContextUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
