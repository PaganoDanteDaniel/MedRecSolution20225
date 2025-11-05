namespace MedRec.Shared.DTOs;

public sealed class ConcurrencyConflictDto
{
    public string EntityName { get; }
    public string PropertyName { get; }
    public object? CurrentValue { get; }
    public object? OriginalValue { get; }

    public ConcurrencyConflictDto(string entityName, string propertyName, object? currentValue, object? originalValue)
    {
        EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        CurrentValue = currentValue;
        OriginalValue = originalValue;
    }
}
