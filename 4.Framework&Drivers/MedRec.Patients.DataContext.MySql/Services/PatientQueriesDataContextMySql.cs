using MedRec.DataContext.MySql.Options;
using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.DataContext.MySql.DataContext;
using MedRec.Patients.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MedRec.Patients.DataContext.MySql.Services;
internal class PatientQueriesDataContextMySql(IOptions<DBOptionsMySql> options)
    : PatientDataContext(options), IPatientQueriesDataContext
{


    public async Task<IEnumerable<Patient>> GetAllPatientsAsync(PaginationDto paginationDTO, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = Patients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(paginationDTO.Filter))
        {
            query = query.Where(p =>
                (p.FirstName.Contains(paginationDTO.Filter) ||
                p.LastName.Contains(paginationDTO.Filter) ||
                p.DocumentNumber.Contains(paginationDTO.Filter) ||
                p.PhoneNumber.Contains(paginationDTO.Filter)) &&
                p.IsDeleted == includeDeleted
            );
        }
        else
        {
            query = query.Where(p => p.IsDeleted == includeDeleted);
        }

        return await query
            .OrderBy(p => p.LastName)
            .Skip((paginationDTO.CurrentPage - 1) * paginationDTO.PageSize)
            .Take(paginationDTO.PageSize)
            .ToListAsync();
    }

    public async Task<Patient> GetPatientByIdAsync(Guid patientId, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = Patients.AsNoTracking().AsQueryable();

        query = query.Where(p => p.IsDeleted == includeDeleted);

        return await query.FirstOrDefaultAsync(p => p.Id == patientId);
    }

    public async Task<Patient> GetPatientByDocNumAsync(string documentNumber, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = Patients.AsNoTracking().AsQueryable();

        query = query.Where(p => p.IsDeleted == includeDeleted);

        return await query.FirstOrDefaultAsync(p => p.DocumentNumber == documentNumber);
    }

    public async Task<int> CountPatientsAsync(string filter = null, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = Patients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(p =>
                (p.FirstName.Contains(filter) ||
                p.LastName.Contains(filter) ||
                p.DocumentNumber.Contains(filter) ||
                p.PhoneNumber.Contains(filter) ||
                p.Email.Contains(filter)) &&
                p.IsDeleted == includeDeleted // Asegúrate de no incluir pacientes eliminados
            );
        }
        else
        {
            query = query.Where(p => p.IsDeleted == includeDeleted); // Asegúrate de no incluir pacientes eliminados
        }

        return await query.CountAsync();
    }

    public async Task<bool> ExistsAsync(Guid patientId, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = Patients.AsNoTracking().AsQueryable();

        query = query.Where(p => p.IsDeleted == includeDeleted);

        return await query.AnyAsync(p => p.Id == patientId);
    }

    public async Task<bool> ExistsAsync(string documentNumber, CancellationToken ct = default, bool includeDeleted = false)
    {
        var query = Patients.AsNoTracking().AsQueryable();

        query = query.Where(p => p.IsDeleted == includeDeleted);
        var response = await query.AnyAsync(p => p.DocumentNumber.Trim() == documentNumber.Trim());
        return response;

    }
}
