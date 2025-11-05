using MedRec.MedicalAppointments.BusinessObjects.DTOs;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface ICreateMedicalAppointmentInputPort
{
    Task Handle(CreateMedicalAppointmentDto createAppointmentDto, CancellationToken ct);
}
