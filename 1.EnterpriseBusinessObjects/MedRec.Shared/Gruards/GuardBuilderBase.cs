using MedRec.Validator.ValueObjects;

namespace MedRec.Shared.Gruards;
public abstract class GuardBuilderBase<TBuilder>
    where TBuilder : GuardBuilderBase<TBuilder>
{
    protected readonly List<ValidationError> _errors = new();
    protected readonly string _paramName;

    protected GuardBuilderBase(string paramName) => _paramName = paramName;

    public IReadOnlyList<ValidationError> Errors => _errors;


    /// <summary>
    /// Indica si este valor es válido (sin errores de validación).
    /// Usar cuando se valida un único campo.
    /// </summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>
    /// Agrega un error personalizado al builder.
    /// </summary>
    protected void AddError(string message)
        => _errors.Add(new ValidationError(_paramName, message));
}
