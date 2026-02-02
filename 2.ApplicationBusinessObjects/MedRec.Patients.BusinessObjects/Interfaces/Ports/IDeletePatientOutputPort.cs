using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IDeletePatientOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken cancellationToken = default);
}
