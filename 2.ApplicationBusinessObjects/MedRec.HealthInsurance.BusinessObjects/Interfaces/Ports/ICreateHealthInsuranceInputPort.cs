using MedRec.HealthInsurance.BusinessObjects.DTOs;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface ICreateHealthInsuranceInputPort
{
    Task Handle(CreateHealthInsuranceDto healthCompany, CancellationToken ct = default);
}
