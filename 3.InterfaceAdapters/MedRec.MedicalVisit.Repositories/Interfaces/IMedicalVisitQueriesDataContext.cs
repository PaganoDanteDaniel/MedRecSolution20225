using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;

namespace MedRec.MedicalVisit.Repositories.Interfaces;
public interface IMedicalVisitQueriesDataContext
{
    Task<PatientMedicalHistory> GetMedicalHistory(Guid patientId, CancellationToken ct = default, bool includeDeleted = false);
    Task<PatientMedicalVisit> GetMedicalVisit(Guid visitId, CancellationToken cts = default, bool includeDeleted = false);
    Task<IEnumerable<PatientMedicalVisit>> GetAllMedicalVisitAsync(Guid patientId, PaginationDto paginationDto = default, CancellationToken cts = default, bool includeDeleted = false);
}
