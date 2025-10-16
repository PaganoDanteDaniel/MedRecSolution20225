using MedRec.BusinessObjects.Interfaces;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface IGetMedicalHistoryIdOutputPort : ICommonOutputPort
{
    Guid HistoryId { get; }

    Task Handle(Guid historyId, CancellationToken cts = default);
}
