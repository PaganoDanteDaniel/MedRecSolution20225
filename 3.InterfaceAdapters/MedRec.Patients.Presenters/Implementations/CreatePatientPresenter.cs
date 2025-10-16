using MedRec.Entity.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.Patients.Presenters.Implementations;
internal class CreatePatientPresenter : ICreatePatientOutputPort
{
    public bool Created { get; private set; }

    public IEnumerable<ValidationError> ValidationErrors { get; private set; } = [];

    public ErrorInfo ErrorMessage { get; private set; }

    public Task ErrorAsync(ErrorInfo message)
    {
        ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task Handle()
    {
        Created = true;
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        ValidationErrors = errors.ToList();
        return Task.CompletedTask;
    }
}
