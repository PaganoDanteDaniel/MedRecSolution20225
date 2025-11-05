using MedRec.Shared.DTOs;
using MedRec.Shared.Exceptions.SQLExceptions;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace MedRec.DataContext.MySql.Guard;

public class GuardDBContext
{
    public static async Task<int> AgainstSaveChangesErrorAsync(
        Func<CancellationToken, Task<int>> saveFunc,
        CancellationToken cancellationToken = default)
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
                "Conflicto de concurrencia detectado al guardar cambios.",
                ex,
                details
            );
        }
        catch (DbUpdateException ex)
        {
            // Intentar detectar duplicados de forma genérica
            if (IsDuplicateKeyError(ex))
            {
                throw new DuplicateKeyException(
                    "Ya existe un registro con un valor duplicado en una clave única.",
                    ex,
                    ExtractEntityNames(ex)
                );
            }

            // Otro error de actualización
            throw new UpdateException(
                "Error al actualizar la base de datos.",
                ex,
                ExtractEntityNames(ex)
            );
        }
        catch (Exception ex)
        {
            throw new UpdateException(
                "Error inesperado en SaveChanges.",
                ex
            );
        }
    }

    private static IReadOnlyList<ConcurrencyConflictDto> MapConcurrencyErrors(DbUpdateConcurrencyException ex)
    {
        var list = new List<ConcurrencyConflictDto>();

        foreach (var entry in ex.Entries)
        {
            var entityName = entry.Entity.GetType().Name;

            // Solo propiedades modificadas participan en la verificación de concurrencia
            foreach (var p in entry.Properties.Where(p => p.IsModified))
            {
                list.Add(new ConcurrencyConflictDto(
                    entityName,
                    p.Metadata.Name,
                    p.CurrentValue,
                    p.OriginalValue
                ));
            }
        }

        return list;
    }

    private static IEnumerable<string> ExtractEntityNames(DbUpdateException ex)
    {
        return ex.Entries.Select(e => e.Entity.GetType().Name);
    }

    /// <summary>
    /// Detecta si el error es por clave duplicada (única o PK) de forma multi-proveedor.
    /// </summary>
    private static bool IsDuplicateKeyError(DbUpdateException ex)
    {
        var inner = ex.InnerException;

        // Recorrer la cadena de excepciones internas
        while (inner != null)
        {
            // Si es una excepción de base de datos común (DbException)
            if (inner is DbException dbEx)
            {
                // Mensajes comunes de duplicado (en inglés o español, según configuración)
                var message = dbEx.Message.ToUpperInvariant();

                // Palabras clave que indican duplicado
                if (message.Contains("DUPLICATE") ||
                    message.Contains("UNIQUE") ||
                    message.Contains("PRIMARY KEY") ||
                    message.Contains("CLAVE") || // en español
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
