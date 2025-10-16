using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface IGetMedicalVisitOutputPort : ICommonOutputPort
{
    GetMedicalVisitDto MedicalVisit { get; }

    Task Handle(PatientMedicalVisit medicalVisit, CancellationToken cts = default);

}
