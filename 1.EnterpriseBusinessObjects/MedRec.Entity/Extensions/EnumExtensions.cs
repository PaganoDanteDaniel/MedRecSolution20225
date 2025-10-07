using System.ComponentModel;
using System.Reflection;

namespace MedRec.Entity.Extensions;
public static class EnumExtensions
{
    /// <summary>
    /// Obtiene el texto del atributo [Description] asociado a un valor de enumeración.
    /// Si no tiene atributo, devuelve el nombre del valor del enum.
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}

/*
<select @bind="selectedStatus">
    @foreach (var status in Enum.GetValues<PatientStatus>())
    {
        <option value="@((int)status)">@status.GetDescription()</option>
    }
</select>

@code {
    private PatientStatus selectedStatus = PatientStatus.Active;

    private void OnSubmit()
    {
        int id = (int)selectedStatus; // ¡Este es el "ID"!
        Console.WriteLine($"ID seleccionado: {id}"); // Ej: 1
    }
}
*/