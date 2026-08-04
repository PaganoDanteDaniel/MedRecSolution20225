using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalVisit.Presenters.Implementations;

internal class GetMedicalHistoryIdPresenter : BaseOutputPort<Guid>, IGetMedicalHistoryIdOutputPort
{
    public Task Handle(Guid historyId, CancellationToken cts = default)
    {
        Result = OperationResult<Guid>.Ok(historyId);
        return Task.CompletedTask;
    }
}
