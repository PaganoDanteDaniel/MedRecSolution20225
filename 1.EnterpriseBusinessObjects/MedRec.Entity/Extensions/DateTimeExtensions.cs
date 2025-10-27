namespace MedRec.Entity.Extensions;
public static class DateTimeExtensions
{
    /// <summary>
    /// Calcula la edad en años basándose en la fecha de nacimiento (DateTime) actual.
    /// </summary>
    /// <param name="dateOfBirth">La instancia de DateTime (fecha de nacimiento).</param>
    /// <returns>La edad calculada en años.</returns>
    public static int CalculateAge(this DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;

        // Retrocede un año si el cumpleaños aún no ha pasado este año
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
    public static string CalculateFullAge(this DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var birthDate = dateOfBirth;

        // Calculating the age
        int year = today.Year - birthDate.Year;
        int month = today.Month - birthDate.Month;
        int day = today.Day - birthDate.Day;

        // Ajuste si el día actual es menor que el d�a de nacimiento
        if (day < 0)
        {
            month--;
            day += DateTime.DaysInMonth(today.Year, today.Month - 1);
        }

        // Ajuste si el mes actual es menor que el mes de nacimiento
        if (month < 0)
        {
            year--;
            month += 12;
        }
        return ($"{year.ToString()} AÑOS, {month.ToString()} MESES");
    }
}
