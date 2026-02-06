using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.DynamicTemplates.Presenters.Implementation;

/// <summary>
/// Presenter for SaveDynamicFields use case.
/// </summary>
public class SaveDynamicFieldsPresenter : BaseOutputPort<int>, ISaveDynamicFieldsOutputPort
{
    public Task Handle(int savedCount)
    {
        Result = OperationResult<int>.Ok(savedCount);
        return Task.CompletedTask;
    }

    public Task HandleValidationErrors(Dictionary<string, List<string>> errors)
    {
        // Mapear Dictionary -> IEnumerable<ValidationError>
        var validationErrors = errors
            .SelectMany(kv => kv.Value.Select(msg => new ValidationError(kv.Key, msg)))
            .ToList()
            .AsEnumerable();

        return ValidationErrorsAsync(validationErrors);
    }
}