namespace MedRec.Shared.Exceptions.SQLExceptions;
public class UpdateException : Exception
{
    public IEnumerable<string> Entities { get; }
    public object Details { get; }

    public UpdateException(string message, Exception inner = null, IEnumerable<string> entities = null, object details = null)
        : base(message, inner)
    {
        Entities = entities;
        Details = details;
    }
}

