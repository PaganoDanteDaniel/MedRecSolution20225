using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

public interface IMedicalVisitSummaryListOutputPort : IBaseOutputPort
{
    OperationResult<IEnumerable<MedicalVisitSummaryDto>> Result { get; }
    Task Handle(IEnumerable<PatientMedicalVisit> listMedicalVisit);
}
