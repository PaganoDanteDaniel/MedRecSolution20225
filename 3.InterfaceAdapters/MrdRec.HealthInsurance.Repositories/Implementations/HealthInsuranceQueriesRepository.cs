using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Entity.Results;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MrdRec.HealthInsurance.Repositories.Implementations;
internal class HealthInsuranceQueriesRepository(IHealthInsuranceQueriesDataContext dataContext) : IHealthInsuranceQueriesRepository
{
    private readonly IHealthInsuranceQueriesDataContext _dataContext = dataContext;

    public async Task<Result<HealthInsuranceCompany>> GetById(Guid id, CancellationToken cancellationToken)
    {
        Result<HealthInsuranceCompany> result = null!;
        try
        {
            if (id == Guid.Empty)
            {
                result = Result<HealthInsuranceCompany>.Fail(new
                    ErrorInfo("Tiene que especificar un identificador para la compañia de salud.", ErrorCode.ValidationError));
            }

            var company = await _dataContext.GetByIdAsync(id, cancellationToken);

            if (company != null)
            {
                result = Result<HealthInsuranceCompany>.Ok(company);
            }
            else
            {
                result = Result<HealthInsuranceCompany>.Fail(new ErrorInfo("No se encontraron datos en la DB.", ErrorCode.NotFound));
            }
        }
        catch (Exception ex)
        {

            result = Result<HealthInsuranceCompany>.Fail(new ErrorInfo("Error al obtener los datos " + ex.Message));
        }

        return result;
    }



    public async Task<Result<IEnumerable<HealthInsuranceCompany>>> GetAll(PaginationDto paginationDto, CancellationToken cancellationToken)
    {
        Result<IEnumerable<HealthInsuranceCompany>> result = null!;
        try
        {
            var catalog = await _dataContext.GetAllAsync(paginationDto, cancellationToken);
            if (catalog != null)
            {
                result = Result<IEnumerable<HealthInsuranceCompany>>.Ok(catalog);
            }
            else
            {
                result = Result<IEnumerable<HealthInsuranceCompany>>.Fail(new ErrorInfo("No se encontraron datos en la DB.", ErrorCode.NotFound));
            }
        }
        catch (Exception ex)
        {
            result = Result<IEnumerable<HealthInsuranceCompany>>.Fail(new ErrorInfo("Error al obtener los datos " + ex.Message, ErrorCode.DatabaseError));
        }
        return result;
    }

    public async Task<Result<int>> GetCount(string filter, CancellationToken cancellationToken)
    {
        Result<int> result = null!;
        try
        {
            var count = await _dataContext.GetTotalCountAsync(filter, cancellationToken);
            result = Result<int>.Ok(count);

        }
        catch (Exception ex)
        {

            result = Result<int>.Fail(new ErrorInfo("Error al obtener los datos " + ex.Message, ErrorCode.DatabaseError));
        }

        return result;
    }
}
