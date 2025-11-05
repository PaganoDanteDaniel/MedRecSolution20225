using MedRec.Entity.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalAppointments.Presenters.Implementations;

internal class CreateMedicalAppointmentPresenter : ICreateMedicalAppointmentOutputPort
{
    private MedicalAppointmentDto? _appointmentDto;
    private ErrorInfo? _errorMessage;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();

    public MedicalAppointmentDto AppointmentDto =>
        _appointmentDto ?? throw new InvalidOperationException("El resultado aún no está disponible. Aún no se ejecutó Handle().");

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage =>
        _errorMessage ?? throw new InvalidOperationException("No hay error disponible. Aún no se ejecutó ErrorAsync().");

    public Task Handle(MedicalAppointmentView appointment, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _appointmentDto = new MedicalAppointmentDto(
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
        _errorMessage = message ?? new ErrorInfo("Error desconocido.");
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        _validationErrors = (errors ?? Enumerable.Empty<ValidationError>()).ToArray();
        return Task.CompletedTask;
    }
}
