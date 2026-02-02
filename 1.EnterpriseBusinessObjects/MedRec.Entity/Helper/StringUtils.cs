using System.Globalization;

namespace MedRec.Entity.Helper;
public static class StringUtils
{
    public static string TitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        // ToTitleCase no capitaliza "small words" en inglés, pero si convertimos a minúsculas primero...
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());

    }
}
