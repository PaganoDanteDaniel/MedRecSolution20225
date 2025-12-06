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
                var dupValue = TryExtractDuplicateValue(ex);
                var msg = dupValue is not null
                    ? $"Ya existe un registro con el valor duplicado: <b>{dupValue}</b>."
                    : "Ya existe un registro con un valor<br /> identico al que desea guardar.";

                throw new DuplicateKeyException(
                    msg,
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
            if (connectionClassifier != null &&
                TryClassifyLostConnection(ex, connectionClassifier, out var lc))
            {
                throw lc;
            }

            throw new UpdateException("Error inesperado en SaveChanges.", ex);
        }
    }

    private static string? TryExtractDuplicateValue(DbUpdateException ex)
    {
        for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is DbException dbEx)
            {
                var msg = dbEx.Message ?? string.Empty;

                // MySQL: "Duplicate entry 'VALUE' for key '...'"
                var mysqlMarker = "Duplicate entry '";
                var i1 = msg.IndexOf(mysqlMarker, StringComparison.OrdinalIgnoreCase);
                if (i1 >= 0)
                {
                    var start = i1 + mysqlMarker.Length;
                    var end = msg.IndexOf('\'', start);
                    if (end > start) return msg.Substring(start, end - start);
                }

                // SQL Server: SqlException.Number 2627 (PK) o 2601 (Unique)
                // Mensaje típico: "Cannot insert duplicate key row in object 'dbo.Table' with unique index 'IX_...'. The duplicate key value is (VALUE)."
                if (dbEx.GetType().Name.Contains("SqlException", StringComparison.OrdinalIgnoreCase))
                {
                    var marker2 = "The duplicate key value is (";
                    var i2 = msg.IndexOf(marker2, StringComparison.OrdinalIgnoreCase);
                    if (i2 >= 0)
                    {
                        var start = i2 + marker2.Length;
                        var end = msg.IndexOf(')', start);
                        if (end > start) return msg.Substring(start, end - start);
                    }
                }

                // PostgreSQL: PostgresException.SqlState == "23505" (unique_violation)
                // Mensaje típico: "duplicate key value violates unique constraint \"...\" Detail: Key (column)=(VALUE) already exists."
                if (dbEx.GetType().Name.Contains("PostgresException", StringComparison.OrdinalIgnoreCase))
                {
                    var detailMarker = "Key ";
                    var i3 = msg.IndexOf(detailMarker, StringComparison.OrdinalIgnoreCase);
                    if (i3 >= 0)
                    {
                        // extraer entre =( y ) ya existe
                        var eq = msg.IndexOf("=(", i3, StringComparison.OrdinalIgnoreCase);
                        var close = msg.IndexOf(')', eq + 2);
                        if (eq >= 0 && close > eq + 2) return msg.Substring(eq + 2, close - (eq + 2));
                    }
                }

                // SQLite: "UNIQUE constraint failed: Table.Column"
                // No aporta el valor duplicado en el mensaje estándar; en ese caso retorna null.
            }
        }
        return null;
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
