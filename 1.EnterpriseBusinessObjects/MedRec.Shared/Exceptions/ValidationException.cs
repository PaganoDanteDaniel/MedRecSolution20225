using MedRec.Entity.ValueObjects;

namespace MedRec.Shared.Exceptions;
public class ValidationException : Exception
{
    public ValidationException()
    {
    }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) :
        base(message, innerException)
    {
    }

    public IEnumerable<ValidationError> Errors { get; }

    public ValidationException(IEnumerable<ValidationError> errors) : base(CreateMessage(errors)) =>
        Errors = errors;

    private static string CreateMessage(IEnumerable<ValidationError> errors) =>
        string.Join("; ", errors.Select(e => $"{e.PropertyName}: {e.Message}"));
}

