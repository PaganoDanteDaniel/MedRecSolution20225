using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;

namespace MedRec.Patients.Repositories.Interfaces;
public interface IPatientQueriesDataContext
{
    Task<IEnumerable<Patient>> GetAllPatientsAsync(PaginationDto paginationDTO, CancellationToken ct = default, bool includeDeleted = false);
    Task<Patient> GetPatientByIdAsync(Guid patientId, CancellationToken ct = default, bool includeDeleted = false);
    Task<Patient> GetPatientByDocNumAsync(string documentNumber, CancellationToken ct = default, bool includeDeleted = false);
    Task<int> CountPatientsAsync(string filter = null, CancellationToken ct = default, bool includeDeleted = false);
    Task<bool> ExistsAsync(Guid patientId, CancellationToken ct = default, bool includeDeleted = false);
    Task<bool> ExistsAsync(string documentNumber, CancellationToken ct = default, bool includeDeleted = false);
}
