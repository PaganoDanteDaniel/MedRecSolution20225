namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface IGetMedicalAppointmentsInputPort
{
    Task Handle((DateTime startDate, DateTime endDate) rangeDate, CancellationToken ct);
}
