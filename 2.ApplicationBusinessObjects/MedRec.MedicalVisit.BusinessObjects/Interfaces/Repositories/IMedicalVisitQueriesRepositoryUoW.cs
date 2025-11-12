using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
public interface IMedicalVisitQueriesRepositoryUoW
{
    Task<Guid> GetMedicalHistory(Guid patientId, CancellationToken cts = default);
    Task<PatientMedicalVisit> GetMedicalVisit(Guid visitId, CancellationToken cts = default);
    Task<IEnumerable<PatientMedicalVisit>> GetMedicalVisits(Guid patientId, PaginationDto paginationDto = default, CancellationToken cts = default);
}
