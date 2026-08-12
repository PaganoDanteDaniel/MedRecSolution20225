using MedRec.DataContext.MySql.Options;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;

namespace MedRec.DataContext.MySql.DataContext;
public class MedRecContext : DbContext, IDisposable
{
    private readonly IOptions<DBOptionsMySql>? _dbOptions;
    private readonly ICurrentUserContext _currentUserContext;
    static int count = 0;
    private static readonly object _logLock = new();
    private static readonly string _logPath = Path.Combine(Path.GetTempPath(), "MedRec", "MedRecContext.log");

    // Constructor principal para producción (inyectado por DI)
    public MedRecContext(IOptions<DBOptionsMySql> dbOptions, ICurrentUserContext currentUserContext)
        : base()
    {
        count += 1;
        _dbOptions = dbOptions ?? throw new ArgumentNullException(nameof(dbOptions));
        _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
        if (string.IsNullOrEmpty(_dbOptions.Value.ConnectionString))
            throw new ArgumentException("Connection string is required.", nameof(dbOptions));
    }

    // Constructor para tiempo de diseño (EF Core tools)
    internal MedRecContext(DbContextOptions<MedRecContext> options)
        : this(options, new NullCurrentUserContext())
    {
    }

    internal MedRecContext(DbContextOptions<MedRecContext> options, ICurrentUserContext currentUserContext)
        : base(options)
    {
        count += 1;
        _currentUserContext = currentUserContext;
    }

    private static void LogCreation(string ctorKind)
    {
        try
        {

            var dir = Path.GetDirectoryName(_logPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Debug.WriteLine($"MedRecContext log path: {_logPath}");

            var ts = DateTime.UtcNow.ToString("o");
            var tid = Thread.CurrentThread.ManagedThreadId;
            var st = new StackTrace(skipFrames: 1, fNeedFileInfo: true).ToString();
            var header = $"********[{ts}] MedRecContext ctor ({ctorKind}) count={count} Thread={tid}********";
            Debug.WriteLine(header);
            Debug.WriteLine(st);

            var text = header + System.Environment.NewLine + st + System.Environment.NewLine + new string('-', 80) + System.Environment.NewLine;
            lock (_logLock)
            {
                File.AppendAllText(_logPath, text);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"No se pudo crear carpeta de log: {ex}");
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
    public DbSet<Professional> Professionals { get; set; }
    public DbSet<MedicalAppointment> MedicalAppointments { get; set; }

    public DbSet<MedicalAppointmentView> MedicalAppointmentsView => Set<MedicalAppointmentView>();
    public DbSet<MedicalSpecialty> MedicalSpecialties { get; set; }
    public DbSet<TemplateFieldDefinition> TemplateFieldDefinitions { get; set; }
    public DbSet<MedicalVisitDynamicField> MedicalVisitDynamicFields { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Solo configura si EF no lo ha hecho ya (por ejemplo, desde la fábrica de diseño)
        if (!optionsBuilder.IsConfigured)
        {
            if (_dbOptions?.Value?.ConnectionString == null)
            {
                throw new InvalidOperationException(
                    "No connection string provided. This context must be configured via " +
                    "IOptions<DBOptionsMySql> (production) or DbContextOptions<MedRecContext> (design-time).");
            }
            optionsBuilder
            .UseMySql(
                _dbOptions.Value.ConnectionString,
                ServerVersion.AutoDetect(_dbOptions.Value.ConnectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromMilliseconds(1000),
                    errorNumbersToAdd: null
                    ));

            //.EnableDetailedErrors()
            //.EnableSensitiveDataLogging() // cuidado: muestra valores (PII) en logs, usar solo en dev
            //.LogTo(
            //    message => Debug.WriteLine(message),
            //    new[] { DbLoggerCategory.Database.Command.Name, DbLoggerCategory.Update.Name },
            //    LogLevel.Information,
            //    DbContextLoggerOptions.SingleLine | DbContextLoggerOptions.UtcTime | DbContextLoggerOptions.Level
            //);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var userId = _currentUserContext.UserId;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                // Evita que EF incluya CreatedAt/CreatedBy en el UPDATE: la entidad en memoria
                // (stub armado desde un DTO, o CurrentValues.SetValues) suele no traer estos
                // valores reales, y sobrescribirlos destruiría la auditoría de creación original.
                entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }

    public override void Dispose()
    {
        count -= 1;
        base.Dispose();
    }
}
