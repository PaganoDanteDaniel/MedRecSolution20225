using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Guard;
using MedRec.Entity.Interfaces;
using MedRec.Shared.Exceptions.SQLExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using System.Data.Common;

namespace MedRec.DataContext.MySql.UnitOfWork;

internal class DataContextUnitOfWork(
    DataBaseContextMySql context,
    IDbConnectionExceptionClassifier connectionClassifier) : IDataContextUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException("Ya existe una transacción activa para este contexto.");

        _currentTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No hay transacción activa para confirmar.");

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No hay transacción activa para deshacer.");

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Centraliza la traducción de errores (concurrencia, duplicados, otros) en GuardDBContext.
        // No hace commit: solo persiste al contexto actual (dentro o fuera de una transacción abierta).
        return GuardDBContext.AgainstSaveChangesErrorAsync(context.SaveChangesAsync, cancellationToken);
    }

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;
        TimeSpan delay = TimeSpan.FromMilliseconds(20);

        for (int attempt = 1; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await operation();
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (IsTransient(ex) && attempt <= maxRetries)
            {
                var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(50, 150));
                await Task.Delay(delay + jitter, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                continue;
            }
            catch (Exception ex)
            {
                // No reclasificar excepciones ya mapeadas por GuardDBContext:
                // si es una excepción de dominio relativa a concurrencia/duplicado/actualización,
                // debe propagarse tal cual para que las capas superiores la manejen correctamente.
                if (ex is ConcurrencyException
                    || ex is DuplicateKeyException
                    || ex is UpdateException
                    || ex is DbUpdateConcurrencyException)
                {
                    throw;
                }

                if (connectionClassifier.TryClassify(ex, out var reason, out var code))
                {
                    var msg = reason switch
                    {
                        LostConnectionReason.UnableToConnect => "No fue posible establecer conexión con el servidor MySQL.",
                        LostConnectionReason.ServerGoneAway => "La conexión con MySQL se perdió (server has gone away).",
                        LostConnectionReason.ConnectionLostDuringQuery => "Se perdió la conexión con MySQL durante la consulta.",
                        LostConnectionReason.TooManyConnections => "El servidor MySQL alcanzó el máximo de conexiones.",
                        LostConnectionReason.StatementInterrupted => "La operación fue interrumpida por MySQL.",
                        LostConnectionReason.Timeout => "La operación excedió el tiempo de espera en MySQL.",
                        _ => "Ocurrió un problema de conexión con MySQL."
                    };

                    throw new LostConnectionException(
                        msg,
                        reason,
                        code,
                        isTransient: IsTransient(ex),
                        innerException: ex);
                }

                throw;
            }
        }
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
