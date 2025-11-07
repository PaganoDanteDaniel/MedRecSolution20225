using MedRec.Entity.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalAppointments.Presenters.Implementations;
internal class ReassignMedicalAppointmentPresenter : IReassignMedicalAppointmentOutputPort
{
    private MedicalAppointmentDto? _dto;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    private ErrorInfo? _error;

    public MedicalAppointmentDto ReassignedAppointmentDto => _dto;

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _error;

    public Task Handle(MedicalAppointmentView appointment, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _dto = new MedicalAppointmentDto(
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

        return Task.CompletedTask;
    }

    public Task ErrorAsync(ErrorInfo message)
    {
        _error = message ?? new ErrorInfo("Error desconocido.");
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        _validationErrors = (errors ?? Enumerable.Empty<ValidationError>()).ToArray();
        return Task.CompletedTask;
    }
}
