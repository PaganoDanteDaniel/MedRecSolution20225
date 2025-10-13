using MedRec.PatientMedicalVisit.BusinessObjects.DTOs;

namespace MedRec.PatientMedicalVisit.BusinessObjects.Interfaces.Ports;
public interface ICreateMedicalVisitInputPort
{
    Task Handle(CreateMedicalVisitDto dto, CancellationToken cts = default);
}
