using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Guard;
using MedRec.Entity.Interfaces;
using MedRec.Shared.Exceptions.SQLExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using System.Data; // <--- agregado para ConnectionState
using System.Data.Common;

namespace MedRec.DataContext.MySql.UnitOfWork;

internal class DataContextUnitOfWork(
    MedRecContext context,
    IDbConnectionExceptionClassifier connectionClassifier) : IDataContextUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException("Ya existe una transacción activa para este contexto.");

        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            try
            {
                await connection.OpenAsync(ct);
            }
            catch (Exception ex)
            {
                // Intentar clasificar el error con tu clasificador
                if (connectionClassifier.TryClassify(ex, out var reason, out var code))
                {
                    var msg = reason switch
                    {
                        LostConnectionReason.UnableToConnect => "No fue posible establecer conexión con el servidor MySQL.",
                        LostConnectionReason.ServerGoneAway => "La conexión con MySQL se perdió.",
                        LostConnectionReason.ConnectionLostDuringQuery => "Se perdió la conexión con MySQL durante la operación.",
                        LostConnectionReason.TooManyConnections => "El servidor MySQL alcanzó el máximo de conexiones.",
                        LostConnectionReason.StatementInterrupted => "La operación fue interrumpida por MySQL.",
                        LostConnectionReason.Timeout => "La operación excedió el tiempo de espera.",
                        _ => "Ocurrió un problema de conexión con MySQL."
                    };

                    throw new LostConnectionException(
                        msg,
                        reason,
                        code,
                        isTransient: IsTransient(ex),
                        innerException: ex);
                }

                // Si no se puede clasificar, lanza una excepción genérica de conexión
                throw new LostConnectionException(
                    "No se pudo abrir la conexión con MySQL.",
                    LostConnectionReason.Unknown,
                    mySqlErrorNumber: null,
                    isTransient: IsTransient(ex),
                    innerException: ex);
            }
        }

        // Ahora la conexión está abierta: iniciar transacción
        _currentTransaction = await context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No hay transacción activa para confirmar.");

        try
        {
            await _currentTransaction.CommitAsync(ct);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No hay transacción activa para deshacer.");

        var connection = context.Database.GetDbConnection();
        var state = connection.State;

        try
        {
            // Solo intentar el rollback si la conexión está realmente abierta.
            // Si está Closed o Broken, evitamos una InvalidOperationException secundaria
            // que ocultaría la causa raíz (p.ej. pérdida de conexión).
            if (state == ConnectionState.Open)
            {
                await _currentTransaction.RollbackAsync(ct);
            }
            else
            {
                // Opcional: logging para diagnóstico (Debug.WriteLine, logger, etc.)
                // Debug.WriteLine($"Rollback omitido: estado de conexión = {state}");
            }
        }
        finally
        {
            DetachAddedEntities();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    private void DetachAddedEntities()
    {
        // Des adjunta todas las entidades en estado Added; útil tras un SaveChanges fallido
        var added = context.ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in added)
        {
            entry.State = EntityState.Detached;
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Centraliza la traducción de errores (concurrencia, duplicados, otros) en GuardDBContext.
        // No hace commit: solo persiste al contexto actual (dentro o fuera de una transacción abierta).
        return GuardDBContext.AgainstSaveChangesErrorAsync(
            context.SaveChangesAsync,
            ct,
            connectionClassifier);
    }

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                ct.ThrowIfCancellationRequested();
                await operation();
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            HandleDatabaseException(ex);
            throw;
        }
    }

    // Sobrecarga opcional: ejecuta bloque transaccional completo bajo strategy (si quieres envolver todo atomicamente).
    public async Task ExecuteInTransactionWithRetryAsync(Func<Task> operation, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                ct.ThrowIfCancellationRequested();
                await BeginTransactionAsync(ct);
                try
                {
                    await operation();
                    await CommitTransactionAsync(ct);
                }
                catch
                {
                    await RollbackTransactionAsync(ct);
                    throw;
                }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            HandleDatabaseException(ex);
            throw;
        }
    }
    private void HandleDatabaseException(Exception ex)
    {
        if (ex is ConcurrencyException
                || ex is DuplicateKeyException
                || ex is UpdateException
                || ex is DbUpdateConcurrencyException
                || ex is LostConnectionException)
        {
            throw ex;
        }

        if (connectionClassifier.TryClassify(ex, out var reason, out var code))
        {
            var msg = reason switch
            {
                LostConnectionReason.UnableToConnect => "No fue posible establecer conexión con el servidor MySQL.",
                LostConnectionReason.ServerGoneAway => "La conexión con MySQL se perdió.",
                LostConnectionReason.ConnectionLostDuringQuery => "Se perdió la conexión con MySQL durante la operación.",
                LostConnectionReason.TooManyConnections => "El servidor MySQL alcanzó el máximo de conexiones.",
                LostConnectionReason.StatementInterrupted => "La operación fue interrumpida por MySQL.",
                LostConnectionReason.Timeout => "La operación excedió el tiempo de espera.",
                _ => "Ocurrió un problema de conexión con MySQL."
            };

            throw new LostConnectionException(
                msg,
                reason,
                code,
                isTransient: IsTransient(ex),
                innerException: ex);
        }

        throw ex;

    }
    private static bool IsTransient(Exception ex)
    {
        // No reintentar conflictos de concurrencia: requieren intervención de la capa superior.
        if (ex is ConcurrencyException ||
            ex is DuplicateKeyException ||
            ex is DbUpdateConcurrencyException)
        {
            return false;
        }

        // Recorrer toda la cadena de excepciones
        for (var inner = ex; inner is not null; inner = inner.InnerException)
        {
            // Manejo específico para errores de MySQL mediante MySqlConnector
            if (inner is MySqlException mySqlEx)
            {
                return mySqlEx.Number switch
                {
                    1213 => true, // Deadlock found when trying to get lock
                    1205 => true, // Lock wait timeout exceeded
                    1040 => true, // Too many connections
                    1042 => true, // Unable to connect to any of the specified MySQL hosts.
                    2002 => true, // Can't connect to local MySQL server
                    2003 => true, // Can't connect to MySQL server on host:port
                    2006 => true, // MySQL server has gone away
                    2013 => true, // Lost connection to MySQL server during query
                    3571 => true, // Statement was interrupted (timeout en algunos contextos)
                    _ => false
                };
            }

            // Fallback opcional: análisis de mensaje (por si algún error raro no es MySqlException)
            // Solo si no es MySqlException, pero sí DbException genérica
            if (inner is DbException dbEx)
            {
                var msg = (dbEx.Message ?? string.Empty).ToUpperInvariant();
                if (msg.Contains("DEADLOCK") ||
                    msg.Contains("LOCK WAIT TIMEOUT") ||
                    msg.Contains("TOO MANY CONNECTIONS") ||
                    msg.Contains("CONNECT") ||
                    msg.Contains("CONNECTION") ||
                    msg.Contains("TIMEOUT"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
