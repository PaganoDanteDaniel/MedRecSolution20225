using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class HealthInsuranceCatalogInteractor(IHealthInsuranceCatalogOutputPort presenter,
    IHealthInsuranceQueriesRepository queriesRepository) : IHealthInsuranceCatalogInputPort
{
    public async Task Handle(PaginationDto pagination, CancellationToken cts)
    {
        cts.ThrowIfCancellationRequested();
        try
        {
            int totalCount = await queriesRepository.GetCount(pagination.FilterOne, cts);
            var result = await queriesRepository.GetAll(pagination, cts);
            await presenter.Handle(result, totalCount, cts);
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
