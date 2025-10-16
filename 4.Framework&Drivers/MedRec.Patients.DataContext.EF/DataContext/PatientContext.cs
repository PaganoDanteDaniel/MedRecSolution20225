using MedRec.DataContext.EF.Configurations;
using MedRec.DataContext.EF.DataContext;
using MedRec.DataContext.EF.Options;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MedRec.Patients.DataContext.EF.DataContext;

public class PatientDataContext : DataBaseContext

{
    public PatientDataContext(IOptions<DBOptions> dbOptions) : base(dbOptions)
    {
    }

    public DbSet<Patient> Patients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PatientConfiguration).Assembly);
    }
}
