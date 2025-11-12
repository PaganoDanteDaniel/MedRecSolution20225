using MedRec.BusinessObjects.Interfaces;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface IUpdateMedicalVisitOutputPort : ICommonOutputPort
{
    bool IsUpdated { get; }
    Task Handle(bool updated, CancellationToken cd);
}
