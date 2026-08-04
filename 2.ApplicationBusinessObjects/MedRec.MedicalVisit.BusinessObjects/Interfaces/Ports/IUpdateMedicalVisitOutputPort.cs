using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

public interface IUpdateMedicalVisitOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(bool updated, CancellationToken cd);
}
