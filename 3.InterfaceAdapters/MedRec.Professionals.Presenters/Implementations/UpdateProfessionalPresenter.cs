using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;

namespace MedRec.Professionals.Presenters.Implementations;
internal class UpdateProfessionalPresenter : BaseOutputPort<bool>, IUpdateProfessionalOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
