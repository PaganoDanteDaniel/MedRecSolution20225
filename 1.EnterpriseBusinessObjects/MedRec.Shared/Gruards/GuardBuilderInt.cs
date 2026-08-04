namespace MedRec.Shared.Gruards;

public class GuardBuilderInt : GuardBuilderBase<GuardBuilderInt>
{
    private readonly int _value;
    private readonly string _paramName;

    public GuardBuilderInt(int value, string paramName) : base(paramName)
    {
        _value = value;
    }

    public GuardBuilderInt LessThan(int comparer, string message = null)
    {
        if (_value < comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser mayor que {comparer}.");

        return this;
    }

    public GuardBuilderInt LessThanOrEqualTo(int comparer, string message = null)
    {
        if (_value <= comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser mayor que {comparer}.");

        return this;
    }
    public GuardBuilderInt GreaterThan(int comparer, string message = null)
    {
        if (_value > comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser menor o igial que {comparer}");

        return this;
    }

    public GuardBuilderInt GreaterThanOrEqualTo(int comparer, string message = null)
    {
        if (_value >= comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser menor que {comparer}.");

        return this;
    }


    public GuardBuilderInt EqualTo(int comparer, string message = null)
    {
        if (_value == comparer)
            AddError(message ?? $"El parámetro '{_paramName}' debe ser distinto a {comparer}.");

        return this;
    }

    public GuardBuilderInt NotEqualTo(int comparer, string message = null)
    {
        if (_value != comparer)
            AddError(message ?? $"El parámetro '{_paramName}' no debe ser igual que {comparer}.");

        return this;
    }
}

