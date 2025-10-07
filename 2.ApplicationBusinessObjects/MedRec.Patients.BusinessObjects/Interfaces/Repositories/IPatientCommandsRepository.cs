using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;

namespace MedRec.Patients.BusinessObjects.Interfaces.Repositories;
public interface IPatientCommandsRepository
{

    Task<Result<Patient>> Create(Patient patient, CancellationToken cancellationToken = default);
    Task<Result<bool>> Update(Patient patient, CancellationToken cancellationToken = default);
    Task<Result<bool>> HardDelete(Guid patientId, CancellationToken cancellationToken = default);
    Task<Result<bool>> SoftDelete(Patient patient, CancellationToken cancellationToken = default);

}
