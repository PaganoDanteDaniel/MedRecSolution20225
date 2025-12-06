using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IUpdateHealthInsuranceOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);
}
