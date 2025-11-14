using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class GetHealthInsuranceByIdInteractor(
    IGetHealthInsuranceByIdOutputPort presenter,
    IHealthInsuranceQueriesRepository queriesRepository) : IGetHealthInsuranceByIdInputPort
{
    public async Task Handle(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "El ID de la obra social es inválido.",
                ErrorCode.ValidationError,
                httpStatusCode: 400));
            return;
        }

        try
        {
            var entity = await queriesRepository.GetById(id, ct);
            await presenter.Handle(entity, ct);
        }
        catch (BusinessException bx)
        {
            await presenter.ErrorAsync(bx.Error);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Ocurrió un error inesperado al obtener los datos.",
                ErrorCode.Unknown,
                new { Exception = ex.Message },
                500));
        }
    }
}
