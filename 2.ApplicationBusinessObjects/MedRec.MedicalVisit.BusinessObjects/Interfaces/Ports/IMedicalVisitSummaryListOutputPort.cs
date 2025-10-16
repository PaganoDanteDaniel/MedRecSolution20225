using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface IMedicalVisitSummaryListOutputPort : ICommonOutputPort
{
    IEnumerable<MedicalVisitSummaryDto> ListMedicalVisitSummary { get; }

    Task Handle(IEnumerable<PatientMedicalVisit> listMedicalVisit);
}
