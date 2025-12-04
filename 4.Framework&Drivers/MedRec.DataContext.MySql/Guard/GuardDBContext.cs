using MedRec.Shared.DTOs;
using MedRec.Shared.Exceptions.SQLExceptions;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace MedRec.DataContext.MySql.Guard;

public static class GuardDBContext
{
    public static async Task<int> AgainstSaveChangesErrorAsync(
        Func<CancellationToken, Task<int>> saveFunc,
        CancellationToken cancellationToken = default,
        IDbConnectionExceptionClassifier? connectionClassifier = null)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await saveFunc(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var details = MapConcurrencyErrors(ex);
            throw new ConcurrencyException(
                "Este registro fue actualizado por otro usuario. Se han cargado los datos más recientes. Revise y guarde nuevamente si es necesario.",
                ex,
                details);
        }
        catch (DbUpdateException ex)
        {
            // Intentar detectar duplicados de forma genérica
            if (IsDuplicateKeyError(ex))
            {
                throw new DuplicateKeyException(
                    "Ya existe un registro con un valor duplicado en una clave única.",
                    ex,
                    ExtractEntityNames(ex));
            }

            // Podría ser un problema de conexión encapsulado en DbUpdateException
            if (connectionClassifier != null &&
                TryClassifyLostConnection(ex, connectionClassifier, out var lc))
            {
                throw lc;
            }

            throw new UpdateException(
                "Error al actualizar la base de datos.",
                ex,
                ExtractEntityNames(ex));
        }
        catch (Exception ex) when (!(ex is ConcurrencyException
                                    || ex is DuplicateKeyException
                                    || ex is UpdateException
                                    || ex is LostConnectionException))
        {
            // Último intento de clasificar conexión antes de generic UpdateException
            if (connectionClassifier != null &&
                TryClassifyLostConnection(ex, connectionClassifier, out var lc))
            {
                throw lc;
            }

            throw new UpdateException("Error inesperado en SaveChanges.", ex);
        }
    }

    private static bool TryClassifyLostConnection(Exception ex,
        IDbConnectionExceptionClassifier classifier,
        out LostConnectionException lost)
    {
        lost = null!;
        if (classifier.TryClassify(ex, out var reason, out var code))
        {
            var msg = reason switch
            {
                LostConnectionReason.UnableToConnect => "No fue posible establecer conexión con el servidor MySQL.",
                LostConnectionReason.ServerGoneAway => "La conexión con MySQL se perdió.",
                LostConnectionReason.ConnectionLostDuringQuery => "Se perdió la conexión con MySQL durante la operación.",
                LostConnectionReason.TooManyConnections => "El servidor alcanzó el máximo de conexiones.",
                LostConnectionReason.StatementInterrupted => "La operación fue interrumpida por MySQL.",
                LostConnectionReason.Timeout => "La operación excedió el tiempo de espera.",
                _ => "Problema de conexión con MySQL."
            };

            lost = new LostConnectionException(
                msg,
                reason,
                code,
                isTransient: true,
                innerException: ex);
            return true;
        }
        return false;
    }

    private static IReadOnlyList<ConcurrencyConflictDto> MapConcurrencyErrors(DbUpdateConcurrencyException ex)
    {
        var list = new List<ConcurrencyConflictDto>();
        if (ex?.Entries == null) return list;

        foreach (var entry in ex.Entries)
        {
            if (entry?.Entity == null) continue;

            // Obtener valores actuales en BD para comparar
            var dbValues = entry.GetDatabaseValues();
            if (dbValues is null)
            {
                // La fila pudo ser eliminada; no hay valores para comparar
                continue;
            }

            // Incluir nueva RowVersion como "conflicto" especial
            var tokenProp = entry.Metadata.GetProperties().FirstOrDefault(p => p.IsConcurrencyToken);
            if (tokenProp is not null)
            {
                var dbToken = dbValues[tokenProp];
                var originalToken = entry.OriginalValues[tokenProp];
                list.Add(new ConcurrencyConflictDto(
                    entry.Entity.GetType().Name,
                    tokenProp.Name,
                    dbToken,
                    originalToken));
            }

            // Reportar solo propiedades con diferencia real entre original y BD
            foreach (var prop in entry.Metadata.GetProperties())
            {
                if (prop.IsConcurrencyToken) continue; // ya incluido arriba

                var original = entry.OriginalValues[prop];
                var database = dbValues[prop];

                // Usa Equals; para tipos referencia puedes ajustar comparación si necesitas
                if (!Equals(original, database))
                {
                    list.Add(new ConcurrencyConflictDto(
                        entry.Entity.GetType().Name,
                        prop.Name,
                        database,
                        original));
                }
            }
        }

        return list;
    }

    private static IEnumerable<string> ExtractEntityNames(DbUpdateException ex) =>
        ex.Entries.Select(e => e.Entity.GetType().Name);

    private static bool IsDuplicateKeyError(DbUpdateException ex)
    {
        var inner = ex.InnerException;
        while (inner != null)
        {
            if (inner is DbException dbEx)
            {
                var message = dbEx.Message.ToUpperInvariant();
                if (message.Contains("DUPLICATE") ||
                    message.Contains("UNIQUE") ||
                    message.Contains("PRIMARY KEY") ||
                    message.Contains("CLAVE") ||
                    message.Contains("DUPLICADA"))
                {
                    return true;
                }
            }
            inner = inner.InnerException;
        }
        return false;
    }
}
