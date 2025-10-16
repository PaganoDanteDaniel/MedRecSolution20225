using MedRec.Shared.Exceptions.SQLExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MedRec.DataContext.EF.Guard;

public class GuardDBContext
{
    public static async Task<int> AgainstSaveChangesErrorAsync(Func<CancellationToken, Task<int>> saveFunc, CancellationToken cancellationToken = default)
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
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
        {
            switch (sqlEx.Number)
            {
                case 2627: // PK duplicada
                case 2601: // Unique index duplicado
                    throw new DuplicateKeyException(
                        "Ya existe un registro con un valor duplicado en una clave única.",
                        ex,
                        ExtractEntityNames(ex)
                    );

                default:
                    throw new UpdateException(
                        "Error en base de datos durante SaveChanges.",
                        ex,
                        ExtractEntityNames(ex)
                    );
            }
        }
        catch (DbUpdateException ex)
        {
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

    private static Dictionary<string, Dictionary<string, (object? Current, object? Original)>> MapConcurrencyErrors(DbUpdateConcurrencyException ex)
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
}


//public class GuardDBContext
//{
//    public static async Task AgainstSaveChangesErrorAsync(DbContext context, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            await context.SaveChangesAsync(cancellationToken);
//        }
//        catch (DbUpdateConcurrencyException ex)
//        {
//            var entityErrors = ex.Entries.ToDictionary(
//                e => e.Entity.GetType().Name, // Nombre de la entidad
//                e => e.Properties.Where(p => p.IsModified).ToDictionary(
//                    p => p.Metadata.Name, // Nombre de la propiedad
//                    p => (p.CurrentValue, p.OriginalValue) // Valores actuales y originales
//                )
//            );

//            if (entityErrors.Any())
//            {
//                throw new UpdateException(
//                    string.Join(", ", entityErrors.Keys), // Nombres de entidades concatenados
//                    entityErrors.SelectMany(e => e.Value).ToDictionary(kv => kv.Key, kv => kv.Value)
//                );
//            }

//            throw; // Esto nunca se ejecutará, pero lo dejamos por seguridad.
//        }
//        catch (DbUpdateException ex)
//        {
//            throw new UpdateException(ex, ex.Entries.Select(e => e.Entity.GetType().Name));
//        }
//        catch (Exception)
//        {
//            throw;
//        }
//    }
//}

