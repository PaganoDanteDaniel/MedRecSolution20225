using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;

namespace MedRec.Patients.BusinessObjects.Interfaces.Repositories;

public interface IPatientQueriesRepository
{
    Task<Result<Patient>> GetPatientById(Guid medicalAppointmentId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Patient>>> GetPatientsList(PaginationDto paginationDTO, CancellationToken cancellationToken = default);
    Task<Result<Patient>> GetPatientByDocumentNumber(string documentNumber, CancellationToken cancellationToken = default);
    Task<Result<int>> CountPatients(string filter, CancellationToken cancellationToken = default);
    Task<Result<bool>> Exists(Guid patientId, CancellationToken cancellationToken = default);
}