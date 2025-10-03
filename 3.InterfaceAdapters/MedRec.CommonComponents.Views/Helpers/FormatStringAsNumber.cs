using System.Globalization;

namespace MedRec.CommonComponents.Views.Helpers;
public static class FormatStringAsNumber
{
    public static string WithDots(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Intenta parsear como número entero (usa long para números grandes)
        if (long.TryParse(input, out long intNumber))
        {
            // Formatea con punto como separador de miles
            return intNumber.ToString("N0", new CultureInfo("es-ES"));
        }

        if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalNumber))
        {
            return decimalNumber.ToString("N", new CultureInfo("es-ES")); // "N" incluye decimales
        }

        // Si no es un número válido, devuelve el valor original
        return input;
    }
}
