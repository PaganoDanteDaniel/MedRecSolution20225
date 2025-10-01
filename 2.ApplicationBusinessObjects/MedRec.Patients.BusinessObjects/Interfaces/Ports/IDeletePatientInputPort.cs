namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IDeletePatientInputPort
{
    Task Handle(Guid deletePatient, CancellationToken cancellationToken = default);
}
