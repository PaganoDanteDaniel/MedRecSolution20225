//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;

//namespace MedRec.DataContext.MySql.DataContext;

//public class MedRecContextMySqlFactory : IDesignTimeDbContextFactory<MedRecContextMySql>
//{
//    public MedRecContextMySql CreateDbContext(string[] args)
//    {
//        var optionsBuilder = new DbContextOptionsBuilder<MedRecContextMySql>();

//        var connectionString =
//            "Server=localhost;Port=3306;Database=medrecdb;User=appuser;Password=MiPass123!;AllowPublicKeyRetrieval=True;SslMode=none";

//        optionsBuilder.UseMySql(
//            connectionString,
//            new MySqlServerVersion(new Version(8, 0, 43))
//        );

//        return new MedRecContextMySql(optionsBuilder.Options);
//    }
//}


