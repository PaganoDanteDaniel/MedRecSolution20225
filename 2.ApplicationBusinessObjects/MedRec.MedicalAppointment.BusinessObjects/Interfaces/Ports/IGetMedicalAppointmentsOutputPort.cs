using MedRec.BusinessObjects.Interfaces;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface IGetMedicalAppointmentsOutputPort : ICommonOutputPort
{
    IEnumerable<MedicalAppointmentDto> AppointmentsDto { get; }

    Task Handle(IEnumerable<MedicalAppointmentView> appointments, CancellationToken ct);
}
