using MedRec.MedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface ICreateMedicalVisitInputPort
{
    Task Handle(CreateMedicalVisitDto dto, CancellationToken cts = default);
}
