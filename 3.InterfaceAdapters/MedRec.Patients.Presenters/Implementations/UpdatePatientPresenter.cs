using MedRec.Entity.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.Patients.Presenters.Implementations;
internal class UpdatePatientPresenter : IUpdatePatientOutputPort
{
    public IEnumerable<ValidationError> ValidationErrors { get; private set; }

    public ErrorInfo ErrorMessage { get; private set; }

    public bool UpdatedSuccessfully { get; private set; }

    public Task ErrorAsync(ErrorInfo message)
    {
        ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task Handle(bool IsUpdated, CancellationToken cancellationToken = default)
    {
        UpdatedSuccessfully = IsUpdated;
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        ValidationErrors = errors;
        return Task.CompletedTask;
    }
}
