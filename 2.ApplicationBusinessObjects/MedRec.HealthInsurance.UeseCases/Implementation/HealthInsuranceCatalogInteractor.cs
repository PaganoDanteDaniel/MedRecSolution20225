using MedRec.Entity.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class HealthInsuranceCatalogInteractor(IHealthInsuranceCatalogOutputPort outputPort,
    IHealthInsuranceQueriesRepository queriesRepository) : IHealthInsuranceCatalogInputPort
{
    private readonly IHealthInsuranceCatalogOutputPort _outputPort = outputPort;
    private readonly IHealthInsuranceQueriesRepository _queriesRepository = queriesRepository;
    public async Task Handle(PaginationDto pagination, CancellationToken cts)
    {
        cts.ThrowIfCancellationRequested();

        // Limpiar el error antes de iniciar la operación
        await _outputPort.ErrorAsync(null);

        var totalResult = await _queriesRepository.GetCount(pagination.FilterOne, cts);
        if (!totalResult.IsSuccess)
        {
            await _outputPort.ErrorAsync(totalResult.Error);
            return;
        }
        int totalCount = totalResult.Value;

        var result = await _queriesRepository.GetAll(pagination, cts);
        if (!result.IsSuccess)
        {
            await _outputPort.ErrorAsync(result.Error);
            return;
        }

        // Limpiar el error antes de mostrar el resultado
        await _outputPort.ErrorAsync(null);
        await _outputPort.Handle(result.Value, totalCount, cts);

    }
}
