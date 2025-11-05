namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface IDeleteMedicalAppointmentInputPort
{
    Task Handle(Guid id, CancellationToken ct);
}
