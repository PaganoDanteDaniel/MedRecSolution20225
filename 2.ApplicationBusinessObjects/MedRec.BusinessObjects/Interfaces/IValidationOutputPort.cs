using MedRec.Validator.ValueObjects;

namespace MedRec.BusinessObjects.Interfaces;
public interface IValidationOutputPort : IErrorOutputPort
{
    IReadOnlyCollection<ValidationError> ValidationErrors { get; }
    Task ValidationErrorsAsync(IEnumerable<ValidationError> errors);
}
