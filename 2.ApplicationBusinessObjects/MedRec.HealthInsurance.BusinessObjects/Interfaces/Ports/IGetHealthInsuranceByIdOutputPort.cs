using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IGetHealthInsuranceByIdOutputPort : IBaseOutputPort
{
    OperationResult<GetHealthInsuranceDto> Result { get; }
    Task Handle(HealthInsuranceCompany healthInsurance, CancellationToken ct = default);
}
