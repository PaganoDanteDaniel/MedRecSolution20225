using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;

namespace MedRec.HealthInsurance.UeseCases.Implementation;
internal class TotalHealthInsuranceCountInteractor(
    ITotalHealthInsuranceCountOutputPort outputPort,
    IHealtInsuranceQueriesRepository queriesRepository) : ITotalHealthInsuranceCountInputPort
{
    private readonly IHealtInsuranceQueriesRepository _queriesRepository = queriesRepository;
    private readonly ITotalHealthInsuranceCountOutputPort _outputPort = outputPort;
    public async Task Handle(string filter = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _outputPort.ErrorAsync(null);

        var result = await _queriesRepository.GetCount(filter, cancellationToken);
        if (!result.IsSuccess)
        {
            await _outputPort.ErrorAsync(result.Error);
        }

        await _outputPort.ErrorAsync(null);
        await _outputPort.Handle(result.Value);

    }
}
