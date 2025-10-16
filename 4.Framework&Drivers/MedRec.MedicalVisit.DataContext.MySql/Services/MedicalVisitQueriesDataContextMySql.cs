using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Options;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MedRec.MedicalVisit.DataContext.MySql.Services;
internal class MedicalVisitQueriesDataContextMySql(IOptions<DBOptionsMySql> options) :
    DataBaseContextMySql(options), IMedicalVisitQueriesDataContext

{
    public async Task<PatientMedicalHistory> GetMedicalHistory(Guid patientId, CancellationToken cts = default, bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        var query = PatientMedicalHistories.AsNoTracking().AsQueryable();

        query = query.Where(p => p.IsDeleted == includeDeleted);

        return await query.FirstOrDefaultAsync(p => p.PatientId == patientId);
    }

    public async Task<IEnumerable<PatientMedicalVisit>> GetAllMedicalVisitAsync(Guid patientId, CancellationToken cts = default, bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        var visits = await (from v in PatientMedicalVisits
                            join h in PatientMedicalHistories
                                on v.MedicalHistoryId equals h.Id
                            join p in Patients
                                on h.PatientId equals p.Id
                            where p.Id == patientId && p.IsDeleted == includeDeleted
                            select v).ToListAsync(cts);
        return visits;
    }

    public async Task<PatientMedicalVisit> GetMedicalVisit(Guid visitId, CancellationToken cts = default, bool includeDeleted = false)
    {
        cts.ThrowIfCancellationRequested();

        var query = PatientMedicalVisits.AsNoTracking().AsQueryable();

        query = query.Where(p => p.IsDeleted == includeDeleted);

        return await query.FirstOrDefaultAsync(p => p.Id == visitId);
    }
}
