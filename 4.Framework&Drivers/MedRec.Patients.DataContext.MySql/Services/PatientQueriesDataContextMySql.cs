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

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync(PaginationDto paginationDTO, CancellationToken ct = default, bool includeDeleted = false)
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
            .ToListAsync(ct);
    }

    public async Task<Patient> GetPatientByIdAsync(Guid patientId, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = _context.Patients.AsNoTracking().Where(p => p.IsDeleted == includeDeleted);
        return await query.FirstOrDefaultAsync(p => p.Id == patientId, ct);
    }

    public async Task<Patient> GetPatientByDocNumAsync(string documentNumber, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = _context.Patients.AsNoTracking().Where(p => p.IsDeleted == includeDeleted);
        return await query.FirstOrDefaultAsync(p => p.DocumentNumber == documentNumber, ct);
    }

    public async Task<int> CountPatientsAsync(string filter = null, CancellationToken ct = default, bool includeDeleted = false)
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

        return await query.CountAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid patientId, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = _context.Patients.AsNoTracking().Where(p => p.IsDeleted == includeDeleted);
        return await query.AnyAsync(p => p.Id == patientId, ct);
    }

    public async Task<bool> ExistsAsync(string documentNumber, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = _context.Patients.AsNoTracking().Where(p => p.IsDeleted == includeDeleted);
        return await query.AnyAsync(p => p.DocumentNumber.Trim() == documentNumber.Trim(), ct);
    }
}
