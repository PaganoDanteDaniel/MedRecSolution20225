using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class UpdateUserPresenter : BaseOutputPort<bool>, IUpdateUserOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
