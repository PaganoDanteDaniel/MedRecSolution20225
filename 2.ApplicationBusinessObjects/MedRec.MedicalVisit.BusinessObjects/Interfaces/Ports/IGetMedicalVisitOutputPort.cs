using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

public interface IGetMedicalVisitOutputPort : IBaseOutputPort
{
    OperationResult<GetMedicalVisitDto> Result { get; }
    Task Handle(PatientMedicalVisit medicalVisit, CancellationToken cts = default);

}
