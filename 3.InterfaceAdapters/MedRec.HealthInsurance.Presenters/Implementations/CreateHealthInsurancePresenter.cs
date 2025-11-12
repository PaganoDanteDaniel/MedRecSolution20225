using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.HealthInsurance.Presenters.Implementations;
internal class CreateHealthInsurancePresenter : ICreateHealthInsuranceOutputPort
{
    private bool _healthCompanyCreated;
    private ErrorInfo? _errorMessage;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    public bool Created => _healthCompanyCreated;

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _errorMessage;

    public Task Handle(HealthInsuranceCompany healthCompany, CancellationToken ct = default)
    {
        if (healthCompany.Id != Guid.Empty)
        {
            _healthCompanyCreated = true;
            return Task.CompletedTask;
        }
        else
        {
            _healthCompanyCreated = false;
            return Task.CompletedTask;
        }
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
