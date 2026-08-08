using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IResetUserPasswordOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);
}
