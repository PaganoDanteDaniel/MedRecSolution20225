namespace MedRec.Shared.Exceptions;

public class ConcurrencyConflictException : Exception
{
    public object CurrentEntity { get; }
    public ConcurrencyConflictException(string message, object currentEntity)
        : base(message)
    {
        CurrentEntity = currentEntity;
    }
}
