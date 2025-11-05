using MedRec.MedicalAppointments.BusinessObjects.DTOs;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface IReassignMedicalAppointmentInputPort
{
    Task Handle(MedicalAppointmentDto reassignAppointmentDto, CancellationToken ct);
}
