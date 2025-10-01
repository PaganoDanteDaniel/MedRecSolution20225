using MedRec.DataContext.EF.Configurations;
using MedRec.DataContext.EF.DataContext;
using MedRec.DataContext.EF.Options;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MedRec.HealthInsurance.DataContext.EF.DataContext;
public class HealthInsuranceContext(IOptions<DBOptions> dbOptions) :
    DataBaseContext(dbOptions)
{
    public DbSet<HealthInsuranceCompany> HealthInsuranceCompanies { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HealthInsuranceCompanyConfiguration).Assembly);
    }
}
