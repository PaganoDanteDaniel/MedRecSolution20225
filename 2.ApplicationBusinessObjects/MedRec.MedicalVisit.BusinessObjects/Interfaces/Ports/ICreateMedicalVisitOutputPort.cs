using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

public interface ICreateMedicalVisitOutputPort : IBaseOutputPort
{
    OperationResult<Guid> Result { get; }
    Task Handle(PatientMedicalVisit visit);

}
