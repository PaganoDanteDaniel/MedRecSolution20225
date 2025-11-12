using MedRec.BusinessObjects.Interfaces;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IUpdateHealthInsuranceOutputPort : ICommonOutputPort
{
    bool IsUpdated { get; }
    Task Handle(bool isUpdated, CancellationToken ct = default);
}
