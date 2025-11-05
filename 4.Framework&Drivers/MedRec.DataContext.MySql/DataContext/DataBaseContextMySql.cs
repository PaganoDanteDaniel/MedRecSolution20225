using MedRec.DataContext.MySql.Options;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace MedRec.DataContext.MySql.DataContext;
public class DataBaseContextMySql(IOptions<DBOptionsMySql> dbOptions) : DbContext
{
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
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<MedicalAppointment> MedicalAppointments { get; set; }

    public DbSet<MedicalAppointmentView> MedicalAppointmentsView => Set<MedicalAppointmentView>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            dbOptions.Value.ConnectionString,
            ServerVersion.AutoDetect(dbOptions.Value.ConnectionString));
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
