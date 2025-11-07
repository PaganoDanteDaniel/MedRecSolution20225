using MedRec.Entity.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalAppointments.Presenters.Implementations;

internal class DeleteMedicalAppointmentPresenter : IDeleteMedicalAppointmentOutputPort
{
    private bool _deleted;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    private ErrorInfo? _error;

    public bool IsDeleted => _deleted;

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _error;

    public Task Handle(bool deleted, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _deleted = deleted;
        return Task.CompletedTask;
    }

    public Task ErrorAsync(ErrorInfo message)
    {
        _error = message ?? new ErrorInfo("Error desconocido.");
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        if (errors is null)
        {
            _validationErrors = Array.Empty<ValidationError>();
        }
        else if (errors is IReadOnlyList<ValidationError> roList)
        {
            _validationErrors = roList;
        }
        else
        {
            _validationErrors = new List<ValidationError>(errors).AsReadOnly();
        }
        return Task.CompletedTask;
    }
}
