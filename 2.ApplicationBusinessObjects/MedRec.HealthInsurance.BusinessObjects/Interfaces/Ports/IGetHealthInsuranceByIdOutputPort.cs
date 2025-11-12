using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IGetHealthInsuranceByIdOutputPort : ICommonOutputPort
{
    GetHealthInsuranceDto HealthInsurance { get; }
    Task Handle(HealthInsuranceCompany healthInsurance, CancellationToken ct = default);
}
