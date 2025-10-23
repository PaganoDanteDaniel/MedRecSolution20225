using MedRec.Entity.Results;

namespace MedRec.Entity.Interfaces;
public interface IUnitOfWork
{
    Task<Result<Unit>> ExecuteTransactionAsync(Func<Task> operation,
       CancellationToken cancellationToken = default);

    Task<Result<T>> ExecuteTransactionAsync<T>(Func<Task<T>> operation,
       CancellationToken cancellationToken = default);
}


