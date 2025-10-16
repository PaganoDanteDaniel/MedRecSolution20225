using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
public interface IMedicalVisitCommandRepository
{
    Task<Result<Unit>> Create(PatientMedicalVisit medicalVisit, CancellationToken cts = default);
    Task<Result<Guid>> CreateMedicalHistory(Guid patientId, CancellationToken cts = default);
}
