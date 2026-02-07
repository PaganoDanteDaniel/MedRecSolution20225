using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

public interface IGetMedicalHistoryIdOutputPort : IBaseOutputPort
{
    OperationResult<Guid> Result { get; }
    Task Handle(Guid historyId, CancellationToken cts = default);
}
