using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalAppointments.Presenters.Implementations;

internal class GetMedicalAppointmentsPresenter : BaseOutputPort<IEnumerable<MedicalAppointmentDto>>, IGetMedicalAppointmentsOutputPort
{
    public Task Handle(IEnumerable<MedicalAppointmentView> appointments, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var result = (appointments ?? Enumerable.Empty<MedicalAppointmentView>())
                    .Select(a => new MedicalAppointmentDto(
                        a.Id,
                        a.AppointmentDateTime,
                        a.PatientId,
                        a.DoctorId,
                        a.Reason ?? string.Empty,
                        a.RowVersion ?? Array.Empty<byte>(),
                        a.IsDeleted,
                        a.PatientFirstName,
                        a.PatientLastName,
                        a.PatientPhoneNumber)).ToList();

        Result = OperationResult<IEnumerable<MedicalAppointmentDto>>.Ok(result);

        return Task.CompletedTask;
    }
}
