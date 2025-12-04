using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;

namespace MedRec.Patients.BusinessObjects.Interfaces.Repositories;

public interface IPatientQueriesRepository
{
    Task<Patient> GetPatientById(Guid medicalAppointmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Patient>> GetPatientsList(PaginationDto paginationDTO, CancellationToken cancellationToken = default);
    Task<Patient> GetPatientByDocumentNumber(string documentNumber, CancellationToken cancellationToken = default);
    Task<int> CountPatients(string filter, CancellationToken cancellationToken = default);
    Task<bool> Exists(Guid patientId, CancellationToken cancellationToken = default);
}