namespace MedRec.Shared.Gruards;
public class GuardBuilderDateTime : GuardBuilderBase<GuardBuilderDateTime>
{
    private readonly DateTime _value;
    private readonly string _paramName;

    public GuardBuilderDateTime(DateTime value, string paramName) : base(paramName)
    {
        _value = value;
    }

    public GuardBuilderDateTime YearEquals(int year, string message = null)
    {
        if (_value.Year != year)
            AddError(message ?? $"El parámetro '{_paramName} debe tener un año igual a {year}");

        return this;
    }

    public GuardBuilderDateTime MonthEquals(int month, string message = null)
    {
        if (_value.Month != month)
            AddError(message ?? $"El parámetro '{_paramName} debe tener un mes igual a {month}");

        return this;
    }

    public GuardBuilderDateTime Before(DateTime minDate, string message = null)
    {
        if (_value < minDate)
            AddError(message ?? $"El parámetro '{_paramName}' no puede ser anterior a {minDate:dd/MM/yyyy}.");

        return this;
    }

    public GuardBuilderDateTime After(DateTime maxDate, string message = null)
    {
        if (_value > maxDate)
            AddError(message ?? $"El parámetro '{_paramName}' no puede ser posterior a {maxDate:dd/MM/yyyy}.");

        return this;
    }

    public GuardBuilderDateTime NotInFuture(string message = null)
    {
        if (_value.Date > DateTime.Now.Date)
            AddError(message ?? $"El parámetro '{_paramName}' no puede estar en el futuro.");

        return this;
    }
    public GuardBuilderDateTime NotNowOrInFuture(string message = null)
    {
        if (_value.Date >= DateTime.Now.Date)
            AddError(message ?? $"El parámetro '{_paramName}' no puede la fecha actual ni estar en el futuro.");

        return this;
    }

    public GuardBuilderDateTime NotInPast(string message = null)
    {
        if (_value < DateTime.Now)
            AddError(message ?? $"El parámetro '{_paramName}' no puede estar en el pasado.");

        return this;
    }

    public GuardBuilderDateTime isRequired(string message = null)
    {
        if (_value == default)
            AddError(message ?? $"Es obligatorio especificar la fecha para asignar la guardia.");

        return this;
    }
}

