namespace MedRec.Validator.ValueObjects;
public class ValidationError(string propertyName, string message)
{
    public string PropertyName => propertyName;

    public string ErrorMessage => message;
}
