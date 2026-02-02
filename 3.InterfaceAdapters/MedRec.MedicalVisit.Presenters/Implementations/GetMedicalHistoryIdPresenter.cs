using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalVisit.Presenters.Implementations;
internal class GetMedicalHistoryIdPresenter : IGetMedicalHistoryIdOutputPort
{
    private Guid _historyId;
    private ErrorInfo? _errorMessage;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    public Guid HistoryId => _historyId;

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _errorMessage;

    public Task ErrorAsync(ErrorInfo message)
    {
        _errorMessage = message ?? new ErrorInfo("Error desconocido.");
        _validationErrors = Array.Empty<ValidationError>();
        _historyId = Guid.Empty;
        return Task.CompletedTask;
    }
    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        _validationErrors = (errors ?? Enumerable.Empty<ValidationError>()).ToArray();
        _errorMessage = null;
        _historyId = Guid.Empty;
        return Task.CompletedTask;
    }
    public Task Handle(Guid historyId, CancellationToken cts = default)
    {
        _errorMessage = null;
        _validationErrors = Array.Empty<ValidationError>();
        _historyId = historyId;
        return Task.CompletedTask;
    }
}
