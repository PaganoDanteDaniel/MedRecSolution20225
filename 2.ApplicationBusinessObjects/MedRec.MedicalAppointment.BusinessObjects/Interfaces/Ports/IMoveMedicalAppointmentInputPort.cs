using MedRec.MedicalAppointments.BusinessObjects.DTOs;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface IMoveMedicalAppointmentInputPort
{
    Task Handle(MoveMedicalAppointmentDto moveAppointmentDto, CancellationToken ct);
}
