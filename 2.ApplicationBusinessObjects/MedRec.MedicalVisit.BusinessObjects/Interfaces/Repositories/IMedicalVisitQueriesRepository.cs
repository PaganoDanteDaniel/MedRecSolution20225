using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
public interface IMedicalVisitQueriesRepository
{
    Task<Result<Guid>> GetMedicalHistory(Guid patientId, CancellationToken cts = default);
    Task<Result<PatientMedicalVisit>> GetMedicalVisit(Guid visitId, CancellationToken cts = default);
    Task<Result<IEnumerable<PatientMedicalVisit>>> GetMedicalVisits(Guid patientId, CancellationToken cts = default);
}
