namespace MedRec.Shared.Gruards;

public class GuardBuilderDecimal : GuardBuilderBase<GuardBuilderDecimal>
{
    private readonly decimal _value;

    public GuardBuilderDecimal(decimal value, string paramName) : base(paramName)
    {
        _value = value;
    }

    public GuardBuilderDecimal GreaterThanOrEqualTo(decimal comparer, string message = null)
    {
        if (_value < comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser mayor o igual que {comparer}.");

        return this;
    }

    public GuardBuilderDecimal LessThanOrEqualTo(decimal comparer, string message = null)
    {
        if (_value > comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser menor o igual que {comparer}.");

        return this;
    }

    public GuardBuilderDecimal GreaterThan(decimal comparer, string message = null)
    {
        if (_value <= comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser mayor que {comparer}.");

        return this;
    }

    public GuardBuilderDecimal LessThan(decimal comparer, string message = null)
    {
        if (_value >= comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser menor que {comparer}.");

        return this;
    }

    public GuardBuilderDecimal EqualTo(decimal comparer, string message = null)
    {
        if (_value == comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser distinto a {comparer}.");

        return this;
    }

    public GuardBuilderDecimal NotEqualTo(decimal comparer, string message = null)
    {
        if (_value != comparer)
            AddError(message ?? $"El parámetro '{_paramName}' no debe ser igual que {comparer}.");

        return this;
    }
}