using MedRec.BusinessObjects.Interfaces;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface ITotalHealthInsuranceCountOutputPort : ICommonOutputPort
{
    int Count { get; }
    Task Handle(int count);
}
