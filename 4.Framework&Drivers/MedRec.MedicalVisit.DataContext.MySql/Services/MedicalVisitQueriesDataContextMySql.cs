using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MedRec.MedicalVisit.DataContext.MySql.Services;

internal class MedicalVisitQueriesDataContextMySql(IServiceScopeFactory scopeFactory) :
    IMedicalVisitQueriesDataContext
{
    public async Task<PatientMedicalHistory?> GetMedicalHistory(Guid patientId,
        CancellationToken cts = default, bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedRecContext>();

        return await context.PatientMedicalHistories
            .AsNoTracking()
            .Where(p => p.IsDeleted == includeDeleted)
            .FirstOrDefaultAsync(p => p.PatientId == patientId, cts);
    }

    public async Task<IEnumerable<PatientMedicalVisit>> GetAllMedicalVisitAsync(
        Guid patientId,
        PaginationDto paginationDto = default,
        CancellationToken cts = default,
        bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedRecContext>();

        var query =
            from v in context.PatientMedicalVisits.AsNoTracking()
            join h in context.PatientMedicalHistories.AsNoTracking() on v.MedicalHistoryId equals h.Id
            join p in context.Patients.AsNoTracking() on h.PatientId equals p.Id
            where p.Id == patientId && p.IsDeleted == includeDeleted
            select v;

        if (!string.IsNullOrWhiteSpace(paginationDto?.FilterOne))
        {
            var filter = paginationDto.FilterOne.Trim();
            if (int.TryParse(filter, out int year) && year is >= 1900 && year <= (DateTime.Now.Year + 1))
            {
                var start = new DateTime(year, 1, 1);
                var end = start.AddYears(1);
                query = query.Where(v => v.VisitDate >= start && v.VisitDate < end);
            }
            else
            {
                query = query.Where(v =>
                    v.Diagnosis.Contains(filter) ||
                    v.Treatment.Contains(filter) ||
                    v.Reason.Contains(filter) ||
                    v.Notes.Contains(filter));
            }
        }

        if (paginationDto is { PageSize: > 0 })
        {
            query = query
                .Skip((paginationDto.CurrentPage - 1) * paginationDto.PageSize)
                .Take(paginationDto.PageSize);
        }

        return await query.ToListAsync(cts);
    }

    public async Task<PatientMedicalVisit?> GetMedicalVisit(Guid visitId,
        CancellationToken cts = default,
        bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MedRecContext>();

        return await context.PatientMedicalVisits
            .AsNoTracking()
            .Where(p => p.IsDeleted == includeDeleted)
            .FirstOrDefaultAsync(p => p.Id == visitId, cts);
    }
}
