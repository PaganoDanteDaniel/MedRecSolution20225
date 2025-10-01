namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface ITotalHealthInsuranceCountInputPort
{
    Task Handle(string filter = null, CancellationToken cancellationToken = default);
}
