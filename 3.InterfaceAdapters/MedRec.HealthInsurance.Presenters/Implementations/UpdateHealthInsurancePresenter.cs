using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;

namespace MedRec.HealthInsurance.Presenters.Implementations;
internal class UpdateHealthInsurancePresenter :
    BaseOutputPort<bool>, IUpdateHealthInsuranceOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
