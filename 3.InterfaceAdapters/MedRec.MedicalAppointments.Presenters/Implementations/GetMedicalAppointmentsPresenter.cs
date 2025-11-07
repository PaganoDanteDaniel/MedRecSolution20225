using MedRec.Entity.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalAppointments.Presenters.Implementations;

internal class GetMedicalAppointmentsPresenter : IGetMedicalAppointmentsOutputPort
{
    private IReadOnlyList<MedicalAppointmentDto>? _appointments;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    private ErrorInfo? _error;

    public IEnumerable<MedicalAppointmentDto> AppointmentsDto => _appointments;

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _error;

    public Task Handle(IEnumerable<MedicalAppointmentView> appointments, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _appointments = (appointments ?? Enumerable.Empty<MedicalAppointmentView>())
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
                a.PatientPhoneNumber))
            .ToArray();

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
