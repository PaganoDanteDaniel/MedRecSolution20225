namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IDeletePatientInputPort
{
    Task Handle(Guid id, CancellationToken ct = default);
}
