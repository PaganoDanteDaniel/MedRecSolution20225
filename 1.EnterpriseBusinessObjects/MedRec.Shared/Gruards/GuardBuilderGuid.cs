namespace MedRec.Shared.Gruards;
public class GuardBuilderGuid : GuardBuilderBase<GuardBuilderGuid>
{
    private readonly Guid? _value;

    public GuardBuilderGuid(Guid? value, string paramName) : base(paramName)
    {
        _value = value;
    }

    /// <summary>
    /// Valida que el Guid no sea null ni Guid.Empty.
    /// </summary>
    public GuardBuilderGuid NotNullOrEmpty(string message = null)
    {
        if (!_value.HasValue)
            AddError(message ?? "El identificador no puede ser nulo.");
        else if (_value.Value == Guid.Empty)
            AddError(message ?? "El identificador no puede estar vacío.");
        return this;
    }
}
