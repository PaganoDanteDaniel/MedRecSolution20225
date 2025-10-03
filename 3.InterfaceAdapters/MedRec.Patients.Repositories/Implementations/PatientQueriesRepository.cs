using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.Repositories.Interfaces;

namespace MedRec.Patients.Repositories.Implementations;
internal class PatientQueriesRepository(IPatientQueriesDataContext queriesDb) : IPatientQueriesRepository
{

    private readonly IPatientQueriesDataContext _queriesDb = queriesDb;
    public async Task<Result<int>> CountPatients(string filter, CancellationToken cancellationToken = default)
    {
        Result<int> result = null!;
        try
        {
            var count = await _queriesDb.CountPatientsAsync(filter, cancellationToken);
            result = Result<int>.Ok(count);

        }
        catch (Exception ex)
        {

            result = Result<int>.Fail(new ErrorInfo("Error al obtener los datos " + ex.Message));
        }

        return result;



    }
    public async Task<Result<bool>> Exists(Guid patientId, CancellationToken cancellationToken = default)
    {
        Result<bool> result = null!;
        try
        {
            result = Result<bool>.Ok(await _queriesDb.ExistsAsync(patientId, cancellationToken));
        }
        catch (Exception ex)
        {

            result = Result<bool>.Fail(new ErrorInfo("Error al obtener los datos " + ex.Message));
        }

        return result;
    }

    public async Task<Result<Patient>> GetPatientByDocumentNumber(string documentNumber, CancellationToken cancellationToken = default)
    {
        Result<Patient> result = null!;
        try
        {
            result = Result<Patient>.Ok(await _queriesDb.GetPatientByDocNumAsync(documentNumber, cancellationToken));
        }
        catch (Exception ex)
        {

            result = Result<Patient>.Fail(new ErrorInfo("Error al obtener los datos " + ex.Message));
        }
        return result;
    }

    public async Task<Result<Patient>> GetPatientById(Guid medicalAppointmentId, CancellationToken cancellationToken = default)
    {
        Result<Patient> result = null!;
        try
        {
            result = Result<Patient>.Ok(await _queriesDb.GetPatientByIdAsync(medicalAppointmentId, cancellationToken));
        }
        catch (Exception ex)
        {

            result = Result<Patient>.Fail(new ErrorInfo("Error al obtener los datos " + ex.Message));
        }
        return result;
    }


    public async Task<Result<IEnumerable<Patient>>> GetPatientsList(PaginationDto paginationDTO, CancellationToken cancellationToken = default)
    {
        try
        {
            var patients = await _queriesDb.GetAllPatientsAsync(paginationDTO, cancellationToken);
            return Result<IEnumerable<Patient>>.Ok(patients);
        }
        catch (Exception ex)
        {
            var error = new ErrorInfo(
                message: "Error al obtener la lista de pacientes.",
                details: ex.Message
            );
            return Result<IEnumerable<Patient>>.Fail(error);
        }
    }
}
