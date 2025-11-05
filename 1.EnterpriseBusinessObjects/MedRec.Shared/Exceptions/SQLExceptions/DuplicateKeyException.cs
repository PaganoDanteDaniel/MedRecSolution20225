namespace MedRec.Shared.Exceptions.SQLExceptions;

public class DuplicateKeyException : UpdateException
{
    public DuplicateKeyException(string message, Exception inner = null, IEnumerable<string> entities = null)
        : base(message, inner, entities) { }
}

