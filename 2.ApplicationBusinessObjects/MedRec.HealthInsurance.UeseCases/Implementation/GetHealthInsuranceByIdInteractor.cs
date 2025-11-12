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
            await presenter.ErrorAsync(new ErrorInfo(
                "Operación cancelada por el usuario.",
                ErrorCode.Cancelled,
                null,
                499));
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
