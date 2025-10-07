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

//public class UpdateException : Exception
//{
//    public UpdateException() { }
//    public UpdateException(string message) : base(message) { }
//    public UpdateException(string message, Exception innerException)
//    : base(message, innerException) { }
//    public UpdateException(
//        string entityName, IDictionary<string, (object CurrentValue, object OriginalValue)> propertyValues)
//    {
//        EntityName = entityName;
//        PropertyValues = propertyValues;
//    }

//    public UpdateException(Exception exception, IEnumerable<string> entities)
//        : base(exception.ErrorMessage, exception) =>
//        Entities = entities;

//    public IEnumerable<string> Entities { get; }
//    public string EntityName { get; init; }
//    public IDictionary<string, (object CurrentValue, object OriginalValue)> PropertyValues { get; init; }
//}

