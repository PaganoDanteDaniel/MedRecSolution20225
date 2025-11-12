using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class TotalHealthInsuranceCountInteractor(
    ITotalHealthInsuranceCountOutputPort presenter,
    IHealthInsuranceQueriesRepository queriesRepository) : ITotalHealthInsuranceCountInputPort
{

    public async Task Handle(string filter = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var count = await queriesRepository.GetCount(filter, cancellationToken);
            await presenter.Handle(count);

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
