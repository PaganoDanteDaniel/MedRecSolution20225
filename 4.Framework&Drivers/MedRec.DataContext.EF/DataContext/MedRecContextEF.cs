using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MedRec.DataContext.EFCore.DataContext;
internal class MedRecContextEF : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=DANTEPC\\MSSQLSERVER01;User ID=sa;Password=MsSql;TrustServerCertificate=True;Initial Catalog=CatalogMedRec"); //Server = (localdb)\\mssqllocaldb; Database = MedRecDB");

    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<PatientMedicalVisit> PatientMedicalVisits { get; set; }
    public DbSet<PatientMedicalCondition> PatientMedicalConditions { get; set; }
    public DbSet<PatientMedicalHistory> PatientMedicalHistories { get; set; }
    public DbSet<MedicalCondition> MedicalConditions { get; set; }
    public DbSet<MedicalConditionType> MedicalConditionTypes { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<HealthInsuranceCompany> HealthInsuranceCompanies { get; set; }
    public DbSet<LaboratoryResultType> LaboratoryResultTypes { get; set; }
    public DbSet<PatientLaboratoryResult> PatientLaboratoryResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
//Add-Migration InitialCreate -p MedRec.DataContext.EF -s MedRec.DataContext.EF -c MedRecContextEF
//Update-Database -p MedRec.DataContext.EF -s MedRec.DataContext.EF -context MedRecContextEF