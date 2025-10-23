using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.Repositories.Interfaces;

namespace MedRec.MedicalVisit.Repositories.Implementations;
internal class MedicalVisitQueriesRepository(IMedicalVisitQueriesDataContext _queriesDataContext) : IMedicalVisitQueriesRepository
{
    public async Task<Result<Guid>> GetMedicalHistory(Guid patientId, CancellationToken cts = default)
    {
        Result<Guid> result = null!;
        try
        {
            var medicalHistory = await _queriesDataContext.GetMedicalHistory(patientId, cts);
            if (medicalHistory != null)
            {
                return result = Result<Guid>.Ok(medicalHistory.Id);

            }

            return result = Result<Guid>.Fail(new ErrorInfo("El paciente no posee historia clínica.", ErrorCode.NotFound));

        }
        catch (Exception ex)
        {

            return result = Result<Guid>.Fail(new ErrorInfo("Error al obtener los datos " + ex.Message));
        }

    }

    public async Task<Result<PatientMedicalVisit>> GetMedicalVisit(Guid visitId, CancellationToken cts = default)
    {
        Result<PatientMedicalVisit> result = null!;
        try
        {
            var medicalVisit = await _queriesDataContext.GetMedicalVisit(visitId, cts);
            if (medicalVisit != null)
            {
                return result = Result<PatientMedicalVisit>.Ok(medicalVisit);
            }
            return result = Result<PatientMedicalVisit>.Fail(new ErrorInfo("El paciente no posee consultas registradas.", ErrorCode.NotFound));

        }
        catch (Exception ex)
        {

            return result = Result<PatientMedicalVisit>.Fail(new ErrorInfo("Error al obtener los datos de las consultas médicas del paciente." + ex.Message));
        }
    }

    public async Task<Result<IEnumerable<PatientMedicalVisit>>> GetMedicalVisits(Guid patientId, PaginationDto paginationDto = default, CancellationToken cts = default)
    {
        Result<IEnumerable<PatientMedicalVisit>> result = null!;
        try
        {
            var medicalVisit = await _queriesDataContext.GetAllMedicalVisitAsync(patientId, paginationDto, cts);
            if (medicalVisit != null)
            {
                return result = Result<IEnumerable<PatientMedicalVisit>>.Ok(medicalVisit);

            }

            return result = Result<IEnumerable<PatientMedicalVisit>>.Fail(new ErrorInfo("El paciente no posee visitas registradas.", ErrorCode.NotFound));

        }
        catch (Exception ex)
        {

            return result = Result<IEnumerable<PatientMedicalVisit>>.Fail(new ErrorInfo("Error al obtener los datos " + ex.Message));
        }
    }
}
