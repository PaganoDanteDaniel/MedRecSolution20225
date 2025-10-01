namespace MedRec.Shared.Gruards;
public class GuardBuilderEnum<TEnum>
        : GuardBuilderBase<GuardBuilderEnum<TEnum>>
        where TEnum : struct, Enum
{
    private readonly object _value;
    private readonly string _paramName;

    public GuardBuilderEnum(object value, string paramName) : base(paramName)
    {
        _value = value;
    }

    public GuardBuilderEnum<TEnum> IsDefined(string message = null)
    {
        if (_value == null || !Enum.IsDefined(typeof(TEnum), _value))
            AddError(message ?? $"El valor del parámetro '{_paramName}' no es válido para el enumerador {typeof(TEnum).Name}.");

        return this;
    }
}
