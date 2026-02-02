namespace MedRec.Shared.DTOs;

public sealed class ConcurrencyConflictDto
{
    public string EntityName { get; }
    public string PropertyName { get; }
    public object? DataBaseValue { get; }
    public object? UserValue { get; }

    public ConcurrencyConflictDto(string entityName, string propertyName, object? dataBaseValue, object? userValue)
    {
        EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        DataBaseValue = dataBaseValue;
        UserValue = userValue;
    }
}
