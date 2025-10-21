using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.MedicalVisit.DataContext.MySql.Services;
internal class MedicalVisitQueriesDataContextMySql(DataBaseContextMySql context) :
    IMedicalVisitQueriesDataContext

{
    public async Task<PatientMedicalHistory> GetMedicalHistory(Guid patientId, CancellationToken cts = default, bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        var query = context.PatientMedicalHistories.AsNoTracking().AsQueryable();

        query = query.Where(p => p.IsDeleted == includeDeleted);

        return await query.FirstOrDefaultAsync(p => p.PatientId == patientId);
    }

    public async Task<IEnumerable<PatientMedicalVisit>> GetAllMedicalVisitAsync(
        Guid patientId, PaginationDto paginationDto = default,
        CancellationToken cts = default, bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        var query = from v in context.PatientMedicalVisits
                    join h in context.PatientMedicalHistories on v.MedicalHistoryId equals h.Id
                    join p in context.Patients on h.PatientId equals p.Id
                    where p.Id == patientId && p.IsDeleted == includeDeleted
                    select v;

        // Aplicar filtro de texto si existe
        if (!string.IsNullOrWhiteSpace(paginationDto?.FilterOne))
        {
            var filterOne = paginationDto?.FilterOne.Trim();

            // 1. Filtrar por AÑO (ej: "2025")
            if (int.TryParse(filterOne, out int year) && year >= 1900 && year <= DateTime.Now.Year + 1)
            {
                var startOfYear = new DateTime(year, 1, 1);
                var startOfNextYear = startOfYear.AddYears(1);

                query = query.Where(v => v.VisitDate >= startOfYear && v.VisitDate < startOfNextYear);
            }
            // 2. Búsqueda en campos de texto clínicos 
            else if (filterOne != null)
            {
                query = query.Where(v =>
                    v.Diagnosis.Contains(filterOne) ||
                    v.Treatment.Contains(filterOne) ||
                    v.Notes.Contains(filterOne));
            }
        }

        if (!string.IsNullOrWhiteSpace(paginationDto?.FilterTwo))
        {
            var filterTwo = paginationDto.FilterTwo.Trim();

            // 3. Filtrar por VALOR NUMÉRICO del enum (ej: "1", "2", "3")
            if (int.TryParse(filterTwo, out int reasonValue) &&
                     Enum.IsDefined(typeof(VisitReason), reasonValue))
            {
                var reason = (VisitReason)reasonValue;
                query = query.Where(v => (int)v.Reason == reasonValue);
            }
        }

        // Aplicar paginación
        if (paginationDto != null && paginationDto.PageSize > 0)
        {
            query = query
                .Skip((paginationDto.CurrentPage - 1) * paginationDto.PageSize)
                .Take(paginationDto.PageSize);
        }

        return await query.ToListAsync(cts);
    }

    public async Task<PatientMedicalVisit> GetMedicalVisit(Guid visitId, CancellationToken cts = default, bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        var query = context.PatientMedicalVisits.AsNoTracking().AsQueryable();

        query = query.Where(p => p.IsDeleted == includeDeleted);

        return await query.FirstOrDefaultAsync(p => p.Id == visitId);
    }
}
