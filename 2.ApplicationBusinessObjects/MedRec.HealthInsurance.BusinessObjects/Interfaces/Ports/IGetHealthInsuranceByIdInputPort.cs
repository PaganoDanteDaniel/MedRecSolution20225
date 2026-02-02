namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IGetHealthInsuranceByIdInputPort
{
    Task Handle(Guid id, CancellationToken ct = default);
}
