using MedRec.BusinessObjects.Interfaces;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface ICreateMedicalAppointmentOutputPort : ICommonOutputPort
{
    MedicalAppointmentDto AppointmentDto { get; }

    Task Handle(MedicalAppointmentView appointment, CancellationToken ct);
}
