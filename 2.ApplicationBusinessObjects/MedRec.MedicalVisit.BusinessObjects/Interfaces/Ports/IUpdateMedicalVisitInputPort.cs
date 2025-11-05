using MedRec.MedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface IUpdateMedicalVisitInputPort
{
    Task Handle(MedicalVisitDto dto, CancellationToken cts = default);
}
