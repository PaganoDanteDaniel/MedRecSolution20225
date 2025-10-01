namespace MedRec.Entity.ValueObjects;
public class ValidationError(string propertyName, string message)
{
    public string PropertyName
    {
        get
        {
            return propertyName;
        }
    }

    public string Message
    {
        get
        {
            return message;
        }
    }
}
