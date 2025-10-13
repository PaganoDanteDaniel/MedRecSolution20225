using MedRec.DataContext.MySql.Configurations;
using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Options;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MedRec.Patients.DataContext.MySql.DataContext;

public class PatientDataContext : DataBaseContextMySql
{
    public PatientDataContext(IOptions<DBOptionsMySql> dbOptions) : base(dbOptions)
    {
    }

    public DbSet<Patient> Patients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PatientConfiguration).Assembly);
    }
}
