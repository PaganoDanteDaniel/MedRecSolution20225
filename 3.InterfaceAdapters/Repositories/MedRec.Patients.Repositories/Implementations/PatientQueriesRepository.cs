using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.Repositories.Interfaces;

namespace MedRec.Patients.Repositories.Implementations;
internal class PatientQueriesRepository(IPatientQueriesDataContext queriesDb) : IPatientQueriesRepository
{

    private readonly IPatientQueriesDataContext _queriesDb = queriesDb;
    public async Task<int> CountPatients(string filter, CancellationToken cancellationToken = default) =>
        await _queriesDb.CountPatientsAsync(filter, cancellationToken);
    public async Task<bool> Exists(Guid patientId, CancellationToken cancellationToken = default) =>
        await _queriesDb.ExistsAsync(patientId, cancellationToken);
    public async Task<Patient> GetPatientByDocumentNumber(string documentNumber, CancellationToken cancellationToken = default) =>
        await _queriesDb.GetPatientByDocNumAsync(documentNumber, cancellationToken);
    public async Task<Patient> GetPatientById(Guid medicalAppointmentId, CancellationToken cancellationToken = default) =>
        await _queriesDb.GetPatientByIdAsync(medicalAppointmentId, cancellationToken);
    public async Task<IEnumerable<Patient>> GetPatientsList(PaginationDto paginationDTO, CancellationToken cancellationToken = default) =>
        await _queriesDb.GetAllPatientsAsync(paginationDTO, cancellationToken);
}
