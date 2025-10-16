using MedRec.DataContext.MySql.Configurations;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;

namespace MedRec.DataContext.MySql.DataContext;

public class MedRecContextMySql : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Cadena de conexión a MySQL
            string connectionString = "Server=localhost;Port=3306;Database=medrecdb;User=appuser;Password=MiPass123!;AllowPublicKeyRetrieval=True;SslMode=none";

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );
        }
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
        modelBuilder.ApplyConfiguration(new CityConfiguration());
        modelBuilder.ApplyConfiguration(new HealthInsuranceCompanyConfiguration());
        modelBuilder.ApplyConfiguration(new LaboratoryResultTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalConditionConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalConditionTypeConfiguration());
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
        modelBuilder.ApplyConfiguration(new PatientLaboratoryResultConfiguration());
        modelBuilder.ApplyConfiguration(new PatientMedicalConditionConfiguration());
        modelBuilder.ApplyConfiguration(new PatientMedicalHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalVisitConfiguration());
        modelBuilder.ApplyConfiguration(new ProvinceConfiguration());
    }
}
//Add-Migration InitialCreate -p MedRec.DataContext.MySql -s MedRec.DataContext.MySql -c MedRecContextMySql
//Update-Database -p MedRec.DataContext.MySql -s MedRec.DataContext.MySql -context MedRecContextMySql
//Remove-Migration -p MedRec.DataContext.MySql -s MedRec.DataContext.MySql -c MedRecContextMySql