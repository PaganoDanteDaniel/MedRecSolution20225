using MedRec.Entity.POCOEntities;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
public interface IMedicalVisitCommandRepositoryUoW
{
    Task Create(PatientMedicalVisit medicalVisit, CancellationToken cts = default);
    Task Update(PatientMedicalVisit medicalVisit, CancellationToken cts = default);
    Task<Guid> CreateMedicalHistory(Guid patientId, CancellationToken cts = default);
}
