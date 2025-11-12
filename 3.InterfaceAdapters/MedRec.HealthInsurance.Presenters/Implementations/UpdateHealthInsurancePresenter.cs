using MedRec.Entity.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.HealthInsurance.Presenters.Implementations;
internal class UpdateHealthInsurancePresenter : IUpdateHealthInsuranceOutputPort
{
    private bool _healthCompanyUpdated;
    private ErrorInfo? _errorMessage;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    public bool IsUpdated => _healthCompanyUpdated;

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _errorMessage;

    public Task Handle(bool isUpdated, CancellationToken ct = default)
    {
        _healthCompanyUpdated = isUpdated;
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
