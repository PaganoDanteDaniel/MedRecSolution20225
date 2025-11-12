namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IDeleteHealthInsuranceInputPort
{
    Task Handle(Guid Id, CancellationToken ct = default);
}
