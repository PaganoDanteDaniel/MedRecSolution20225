using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalAppointments.Presenters.Implementations;

internal class MoveMedicalAppointmentPresenter : BaseOutputPort<MedicalAppointmentDto>, IMoveMedicalAppointmentOutputPort
{
    public Task Handle(MedicalAppointmentView appointment, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var dto = new MedicalAppointmentDto(
            appointment.Id,
            appointment.AppointmentDateTime,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.Reason ?? string.Empty,
            appointment.RowVersion ?? Array.Empty<byte>(),
            appointment.IsDeleted,
            appointment.PatientFirstName,
            appointment.PatientLastName,
            appointment.PatientPhoneNumber);

        Result = OperationResult.Ok(dto);

        return Task.CompletedTask;
    }
}
