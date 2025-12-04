using System.Text.RegularExpressions;

namespace MedRec.Shared.Gruards;

public class GuardBuilderString : GuardBuilderBase<GuardBuilderString>
{
    private readonly string _value;
    private readonly string _paramName;

    public GuardBuilderString(string value, string paramName) : base(paramName)
    {
        _value = value;
        _paramName = paramName;
    }


    public GuardBuilderString NotNullOrEmpty(string mensaje = null)
    {
        if (string.IsNullOrWhiteSpace(_value))
            AddError(mensaje ?? $"El parámetro '{_paramName}' es requerido.");
        return this;
    }

    public GuardBuilderString NonNumeric(string mensaje = null)
    {
        if (!Regex.IsMatch(_value ?? "", @"^\d+$"))
            AddError(mensaje ?? $"El parámetro '{_paramName}' debe contener solo números.");
        return this;
    }

    public GuardBuilderString MinLength(int minLength, string mensaje = null)
    {
        if (_value?.Length < minLength)
            AddError(mensaje ?? $"El parámetro '{_paramName}' debe tener al menos {minLength} caracteres.");
        return this;
    }

    public GuardBuilderString MaxLength(int maxLength, string mensaje = null)
    {
        if (_value?.Length > maxLength)
            AddError(mensaje ?? $"El parámetro '{_paramName}' no puede superar {maxLength} caracteres.");
        return this;
    }

    public GuardBuilderString InvalidEmail(string mensaje = null)
    {
        if (string.IsNullOrWhiteSpace(_value))
        {
            AddError(mensaje ?? $"El parámetro '{_paramName}' no puede estar vacío.");
        }
        else
        {
            var emailRegex = new Regex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$");
            if (!emailRegex.IsMatch(_value))
                AddError(mensaje ?? $"El parámetro '{_paramName}' no es un correo válido.");
        }
        return this;
    }
}


