using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.Repositories.Interfaces;

namespace MedRec.MedicalVisit.Repositories.Implementations;
internal class MedicalVisitQueriesRepositoryUoW(IMedicalVisitQueriesDataContext queriesDataContext) : IMedicalVisitQueriesRepositoryUoW
{
    public async Task<Guid> GetMedicalHistory(Guid patientId, CancellationToken ct = default)
    {
        var entity = await queriesDataContext.GetMedicalHistory(patientId, ct);
        return entity?.Id ?? Guid.Empty;
    }

    public async Task<PatientMedicalVisit> GetMedicalVisit(Guid visitId, CancellationToken cts = default) =>
        await queriesDataContext.GetMedicalVisit(visitId, cts);

    public async Task<IEnumerable<PatientMedicalVisit>> GetMedicalVisits(Guid patientId,
        PaginationDto paginationDto = null,
        CancellationToken ct = default) =>
        await queriesDataContext.GetAllMedicalVisitAsync(patientId, paginationDto, ct);
}
