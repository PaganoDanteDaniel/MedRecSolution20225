using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface ICreatePatientOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);    // Notifica éxito

}
