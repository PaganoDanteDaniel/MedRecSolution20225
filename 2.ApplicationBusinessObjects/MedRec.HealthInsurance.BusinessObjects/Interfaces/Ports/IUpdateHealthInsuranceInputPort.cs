using MedRec.HealthInsurance.BusinessObjects.DTOs;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IUpdateHealthInsuranceInputPort
{
    Task Handle(UpdateHealthInsuranceDto healthInsuranceDto, CancellationToken ct = default);
}
