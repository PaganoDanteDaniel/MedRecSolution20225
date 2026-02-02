using MedRec.Entity.POCOEntities;

namespace MedRec.Patients.BusinessObjects.Interfaces.Repositories;
public interface IPatientCommandsRepository
{

    Task Create(Patient patient, CancellationToken cancellationToken = default);
    Task Update(Patient patient, CancellationToken cancellationToken = default);
    Task HardDelete(Guid patientId, CancellationToken cancellationToken = default);
    Task SoftDelete(Patient patient, CancellationToken cancellationToken = default);

}
