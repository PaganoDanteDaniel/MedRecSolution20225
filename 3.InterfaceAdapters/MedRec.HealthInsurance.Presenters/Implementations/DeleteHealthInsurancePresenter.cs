using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.HealthInsurance.Presenters.Implementations;
internal class DeleteHealthInsurancePresenter : IDeleteHealthInsuranceOutputPort
{
    private bool _deleted;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    private ErrorInfo? _error;

    public bool IsDeleted => _deleted;

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _error;

    public Task ErrorAsync(ErrorInfo message)
    {
        _error = message;
        return Task.CompletedTask;
    }

    public Task Handle(HealthInsuranceCompany healthInsurance, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (healthInsurance.IsDeleted)
        {
            _deleted = true;
        }
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
