using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class GetMedicalVisitInteractor(
    IGetMedicalVisitOutputPort _outputPort,
    IMedicalVisitQueriesRepository _repository) : IGetMedicalVisitInputPort
{
    public async Task Handle(Guid medicalVisitId, CancellationToken cts = default)
    {
        var result = await _repository.GetMedicalVisit(medicalVisitId, cts).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            await _outputPort.ErrorAsync(result.Error);
            return;
        }

        await _outputPort.Handle(result.Value);
    }
}
