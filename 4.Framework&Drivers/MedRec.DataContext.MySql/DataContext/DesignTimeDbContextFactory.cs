using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedRec.DataContext.MySql.DataContext;
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MedRecContext>
{
    public MedRecContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MedRecContext>();

        // Usa una cadena de conexión de desarrollo (la misma que usabas antes)
        var connectionString = "Server=localhost;Port=3306;Database=medrecdb;User=appuser;Password=MiPass123!;AllowPublicKeyRetrieval=True;SslMode=none";

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString));

        return new MedRecContext(optionsBuilder.Options);
    }
}
