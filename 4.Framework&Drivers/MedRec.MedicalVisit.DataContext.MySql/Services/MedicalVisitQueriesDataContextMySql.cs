using MedRec.DataContext.MySql.DataContext;
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

    public async Task<IEnumerable<PatientMedicalVisit>> GetAllMedicalVisitAsync(Guid patientId, CancellationToken cts = default, bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        var visits = await (from v in context.PatientMedicalVisits
                            join h in context.PatientMedicalHistories
                                on v.MedicalHistoryId equals h.Id
                            join p in context.Patients
                                on h.PatientId equals p.Id
                            where p.Id == patientId && p.IsDeleted == includeDeleted
                            select v).ToListAsync(cts);
        return visits;
    }

    public async Task<PatientMedicalVisit> GetMedicalVisit(Guid visitId, CancellationToken cts = default, bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        var query = context.PatientMedicalVisits.AsNoTracking().AsQueryable();

        query = query.Where(p => p.IsDeleted == includeDeleted);

        return await query.FirstOrDefaultAsync(p => p.Id == visitId);
    }
}
