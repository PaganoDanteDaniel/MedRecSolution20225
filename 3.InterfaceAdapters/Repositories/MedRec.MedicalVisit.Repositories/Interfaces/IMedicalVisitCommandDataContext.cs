using MedRec.Entity.POCOEntities;

namespace MedRec.MedicalVisit.Repositories.Interfaces;
public interface IMedicalVisitCommandDataContext
{
    Task CreateAsync(PatientMedicalVisit medicalVisit, CancellationToken cts = default);
    Task UpdateAsync(PatientMedicalVisit medicalVisit, CancellationToken cts = default);
    Task CreateMedicalHistoryAsync(PatientMedicalHistory medHist, CancellationToken cts = default);


    // NUEVOS métodos para soportar la lógica de creación segura
    Task<bool> HasMedicalHistoryAsync(Guid patientId, CancellationToken ct = default);
    Task<Guid> GetMedicalHistoryIdByPatientIdAsync(Guid patientId, CancellationToken ct = default);
}
