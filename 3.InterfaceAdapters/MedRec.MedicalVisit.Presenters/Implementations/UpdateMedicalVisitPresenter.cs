using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalVisit.Presenters.Implementations;
internal class UpdateMedicalVisitPresenter : IUpdateMedicalVisitOutputPort
{
    bool _updated;
    private IEnumerable<ValidationError> _validationErrors;
    private ErrorInfo _errorInfo;
    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _errorInfo;

    public bool IsUpdated => _updated;

    public Task ErrorAsync(ErrorInfo message)
    {
        _errorInfo = message;
        return Task.CompletedTask;
    }

    public Task Handle(bool updated, CancellationToken cd)
    {
        if (updated)
            _updated = true;
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        _validationErrors = errors.ToList();
        return Task.CompletedTask;
    }
}
