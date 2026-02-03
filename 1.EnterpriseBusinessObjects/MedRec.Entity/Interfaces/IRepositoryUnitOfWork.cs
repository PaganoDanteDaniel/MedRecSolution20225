namespace MedRec.Entity.Interfaces;
public interface IRepositoryUnitOfWork : IDisposable
{
    Task BeginTransaction(CancellationToken ct = default);
    Task CommitTransaction(CancellationToken ct = default);
    Task RollbackTransaction(CancellationToken ct = default);

    /// <summary>
    /// Guarda los cambios pendientes en el contexto de base de datos.
    /// Las excepciones de BD (concurrencia, duplicados, conexión) son traducidas
    /// automáticamente a excepciones de dominio por la implementación.
    /// </summary>
    /// <exception cref="ConcurrencyException">Cuando hay conflicto optimista.</exception>
    /// <exception cref="DuplicateKeyException">Cuando se viola restricción única.</exception>
    /// <exception cref="LostConnectionException">Cuando falla la conexión.</exception>
    Task<int> SaveChanges(CancellationToken ct = default);
    Task ExecuteWithRetry(Func<Task> operation, CancellationToken ct = default);
    Task ExecuteInTransactionWithRetry(Func<Task> work, CancellationToken ct = default);
}
