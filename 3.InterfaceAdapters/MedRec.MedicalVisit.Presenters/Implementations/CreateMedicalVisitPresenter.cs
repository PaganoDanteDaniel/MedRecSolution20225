using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalVisit.Presenters.Implementations;
internal class CreateMedicalVisitPresenter : ICreateMedicalVisitOutputPort
{
    private bool _created;
    private ErrorInfo? _errorMessage;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    public bool Created => _created;

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    // Devuelve un ErrorInfo no nulo; usamos un fallback si no hay error.
    public ErrorInfo ErrorMessage => _errorMessage ?? new ErrorInfo(string.Empty);

    public Task Handle()
    {
        // Éxito: limpiar errores previos y marcar creado
        _errorMessage = null;
        _validationErrors = Array.Empty<ValidationError>();
        _created = true;
        return Task.CompletedTask;
    }

    public Task ErrorAsync(ErrorInfo message)
    {
        // Error: guardar mensaje y limpiar validaciones
        _created = false;
        _errorMessage = message ?? new ErrorInfo("Error desconocido.");
        _validationErrors = Array.Empty<ValidationError>();
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        // Validaciones: guardar y limpiar mensaje de error
        _created = false;
        _validationErrors = (errors ?? Enumerable.Empty<ValidationError>()).ToArray();
        _errorMessage = null;
        return Task.CompletedTask;
    }
}
