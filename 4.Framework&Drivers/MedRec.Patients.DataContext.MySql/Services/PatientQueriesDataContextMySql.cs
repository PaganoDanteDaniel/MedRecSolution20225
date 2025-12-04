using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.Repositories.Interfaces;
using MedRec.Shared.Exceptions.SQLExceptions;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Patients.DataContext.MySql.Services;
internal class PatientQueriesDataContextMySql(MedRecContext context, IDbConnectionExceptionClassifier classifier)
    : IPatientQueriesDataContext
{
    private readonly MedRecContext _context = context;
    private readonly IDbConnectionExceptionClassifier _classifier = classifier;

    private async Task<T> ExecuteQueryAsync<T>(Func<CancellationToken, Task<T>> query, CancellationToken ct)
    {
        try
        {
            // Usa la ExecutionStrategy del proveedor. Si no soporta reintentos, actúa como no-retry.
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () => await query(ct));
        }
        catch (Exception ex)
        {
            if (_classifier.TryClassify(ex, out var reason, out var code))
            {
                var msg = reason switch
                {
                    LostConnectionReason.UnableToConnect => "No fue posible establecer conexión con el servidor MySQL.",
                    LostConnectionReason.ServerGoneAway => "La conexión con MySQL se perdió.",
                    LostConnectionReason.ConnectionLostDuringQuery => "Se perdió la conexión con MySQL durante la consulta.",
                    LostConnectionReason.TooManyConnections => "El servidor MySQL alcanzó el máximo de conexiones.",
                    LostConnectionReason.StatementInterrupted => "La operación fue interrumpida por MySQL.",
                    LostConnectionReason.Timeout => "La operación excedió el tiempo de espera.",
                    _ => "Problema de conexión con MySQL."
                };
                throw new LostConnectionException(msg, reason, code, isTransient: true, innerException: ex);
            }
            throw;
        }
    }

    public Task<IEnumerable<Patient>> GetAllPatientsAsync(PaginationDto paginationDTO, CancellationToken ct = default, bool includeDeleted = false)
        => ExecuteQueryAsync<IEnumerable<Patient>>(async token =>
        {
            var query = _context.Patients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(paginationDTO.FilterOne))
            {
                query = query.Where(p =>
                    (p.FirstName.Contains(paginationDTO.FilterOne) ||
                     p.LastName.Contains(paginationDTO.FilterOne) ||
                     p.DocumentNumber.Contains(paginationDTO.FilterOne) ||
                     p.PhoneNumber.Contains(paginationDTO.FilterOne)) &&
                    p.IsDeleted == includeDeleted);
            }
            else
            {
                query = query.Where(p => p.IsDeleted == includeDeleted);
            }

            return await query
                .OrderBy(p => p.LastName)
                .Skip((paginationDTO.CurrentPage - 1) * paginationDTO.PageSize)
                .Take(paginationDTO.PageSize)
                .ToListAsync(token);
        }, ct);

    public Task<Patient> GetPatientByIdAsync(Guid patientId, CancellationToken ct = default, bool includeDeleted = false)
        => ExecuteQueryAsync<Patient>(async token =>
        {
            var query = _context.Patients.AsNoTracking().Where(p => p.IsDeleted == includeDeleted);
            return await query.FirstOrDefaultAsync(p => p.Id == patientId, token);
        }, ct);

    public Task<Patient> GetPatientByDocNumAsync(string documentNumber, CancellationToken ct = default, bool includeDeleted = false)
        => ExecuteQueryAsync<Patient>(async token =>
        {
            var query = _context.Patients.AsNoTracking().Where(p => p.IsDeleted == includeDeleted);
            return await query.FirstOrDefaultAsync(p => p.DocumentNumber == documentNumber, token);
        }, ct);

    public Task<int> CountPatientsAsync(string filter = null, CancellationToken ct = default, bool includeDeleted = false)
        => ExecuteQueryAsync<int>(async token =>
        {
            var query = _context.Patients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(p =>
                    (p.FirstName.Contains(filter) ||
                     p.LastName.Contains(filter) ||
                     p.DocumentNumber.Contains(filter) ||
                     p.PhoneNumber.Contains(filter) ||
                     p.Email.Contains(filter)) &&
                    p.IsDeleted == includeDeleted);
            }
            else
            {
                query = query.Where(p => p.IsDeleted == includeDeleted);
            }

            return await query.CountAsync(token);
        }, ct);

    public Task<bool> ExistsAsync(Guid patientId, CancellationToken ct = default, bool includeDeleted = false)
        => ExecuteQueryAsync<bool>(async token =>
        {
            var query = _context.Patients.AsNoTracking().Where(p => p.IsDeleted == includeDeleted);
            return await query.AnyAsync(p => p.Id == patientId, token);
        }, ct);

    public Task<bool> ExistsAsync(string documentNumber, CancellationToken ct = default, bool includeDeleted = false)
        => ExecuteQueryAsync<bool>(async token =>
        {
            var query = _context.Patients.AsNoTracking().Where(p => p.IsDeleted == includeDeleted);
            return await query.AnyAsync(p => p.DocumentNumber.Trim() == documentNumber.Trim(), token);
        }, ct);
}
