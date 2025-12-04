using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.DTOs;
using MedRec.Validator.ValueObjects;

namespace MedRec.BusinessObjects.Abstracts;
public abstract class ErrorAndValidationOutputPort : IValidationOutputPort
{
    private ErrorInfo? _errorMessage;
    private IReadOnlyCollection<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    public ErrorInfo? ErrorMessage => _errorMessage;
    public IReadOnlyCollection<ValidationError> ValidationErrors => _validationErrors;

    public Task ErrorAsync(ErrorInfo message)
    {
        _errorMessage = message;
        return Task.CompletedTask;
    }
    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        _validationErrors = errors.ToList();
        return Task.CompletedTask;
    }
}
