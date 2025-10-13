using MedRec.Shared.Exceptions.SQLExceptions;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace MedRec.DataContext.MySql.Guard;

public class GuardDBContext
{
    public static async Task<int> AgainstSaveChangesErrorAsync(
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await context.SaveChangesAsync(cancellationToken);
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

    private static Dictionary<string, Dictionary<string, (object? Current, object? Original)>>
        MapConcurrencyErrors(DbUpdateConcurrencyException ex)
    {
        return ex.Entries.ToDictionary(
            e => e.Entity.GetType().Name,
            e => e.Properties
                .Where(p => p.IsModified)
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => (p.CurrentValue, p.OriginalValue)
                )
        );
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
