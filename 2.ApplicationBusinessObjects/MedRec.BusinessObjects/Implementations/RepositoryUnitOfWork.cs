using MedRec.Entity.Interfaces;

namespace MedRec.BusinessObjects.Implementations;
internal class RepositoryUnitOfWork(IDataContextUnitOfWork context, CancellationToken cts = default) : IRepositoryUnitOfWork
{
    public async Task BeginTransactionAsync(CancellationToken ct = default) =>
        await context.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default) =>
        await context.CommitTransactionAsync(ct);

    public async Task RollbackAsync(CancellationToken ct = default) =>
        await context.RollbackTransactionAsync(ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
             await context.SaveChangesAsync(ct);

    public void Dispose() => (context as IDisposable)?.Dispose();
}
