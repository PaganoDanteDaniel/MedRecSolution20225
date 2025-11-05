using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.MedicalVisit.Repositories.Interfaces;
public interface IMedicalVisitCommandDataContext : IDataContextUnitOfWork
{
    Task CreateAsync(PatientMedicalVisit medicalVisit, CancellationToken cts = default);
    Task UpdateAsync(PatientMedicalVisit medicalVisit, CancellationToken cts = default);
    Task CreateMedicalHistoryAsync(PatientMedicalHistory medHist, CancellationToken cts = default);
}
