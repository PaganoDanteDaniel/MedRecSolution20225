using MedRec.DataContext.MySql.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MedRec.DataContext.MySql.DataContext;
public class DataBaseContextMySql(IOptions<DBOptionsMySql> dbOptions) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            dbOptions.Value.ConnectionString,
            ServerVersion.AutoDetect(dbOptions.Value.ConnectionString), // Detecta versión automáticamente
            options =>
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
    }
}
