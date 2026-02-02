using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalVisit.Presenters.Implementations;
internal class UpdateMedicalVisitPresenter : IUpdateMedicalVisitOutputPort
{
    private bool _updated;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    private ErrorInfo? _errorMessage;
    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _errorMessage;

    public bool IsUpdated => _updated;

    public Task ErrorAsync(ErrorInfo message)
    {
        if (message.Code == ErrorCode.ConcurrencyError)
        {
            //message.Details
        }



        _updated = false;
        _errorMessage = message ?? new ErrorInfo("Error desconocido.");
        _validationErrors = Array.Empty<ValidationError>();
        return Task.CompletedTask;
    }
    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        _updated = false;
        _validationErrors = (errors ?? Enumerable.Empty<ValidationError>()).ToArray();
        _errorMessage = null;
        return Task.CompletedTask;
    }

    public Task Handle(bool updated, CancellationToken cd)
    {
        _errorMessage = null;
        _validationErrors = Array.Empty<ValidationError>();
        if (updated)
            _updated = true;
        return Task.CompletedTask;
    }
}
