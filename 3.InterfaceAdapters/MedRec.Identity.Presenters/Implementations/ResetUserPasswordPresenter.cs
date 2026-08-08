using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class ResetUserPasswordPresenter : BaseOutputPort<bool>, IResetUserPasswordOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
