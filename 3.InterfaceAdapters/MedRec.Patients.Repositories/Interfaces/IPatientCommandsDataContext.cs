using MedRec.Entity.POCOEntities;

namespace MedRec.Patients.Repositories.Interfaces;
public interface IPatientCommandsDataContext
{
    Task CreatePatientAsync(Patient patient, CancellationToken cancellationToken = default);
    Task UpdatePatientAsync(Patient patient, CancellationToken cancellationToken = default);
    Task HardDeletePatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task SoftDeletePatientAsync(Patient patient, CancellationToken cancellationToken = default);
}
