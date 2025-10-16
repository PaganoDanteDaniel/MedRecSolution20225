using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalVisit.Presenters.Implementations;
internal class GetMedicalHistoryIdPresenter : IGetMedicalHistoryIdOutputPort
{
    public Guid HistoryId { get; private set; }

    public IEnumerable<ValidationError> ValidationErrors { get; private set; }

    public ErrorInfo ErrorMessage { get; private set; }

    public Task ErrorAsync(ErrorInfo message)
    {
        ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task Handle(Guid historyId, CancellationToken cts = default)
    {
        HistoryId = historyId;
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        ValidationErrors = errors;
        return Task.CompletedTask;
    }
}
