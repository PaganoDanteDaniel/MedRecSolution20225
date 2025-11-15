using MedRec.Shared.DTOs;
using System.Collections.Concurrent;
using System.Reflection;

namespace MedRec.Shared.Helpers;
public static class ConcurrencyHelper
{
    // Caché global de propiedades por tipo para evitar reflexiones repetidas.
    // Clave: Type ; Valor: Dictionary que mapea nombre de propiedad (case-insensitive) => PropertyInfo
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache
        = new();

    /// <summary>
    /// Aplica los valores "Current" de conflictos de concurrencia sobre el modelo.
    /// Se usa reflexión pero con caché para minimizar coste.
    /// </summary>
    public static void ApplyCurrentValues<TModel>(this TModel model, IEnumerable<ConcurrencyConflictDto> conflicts)
        where TModel : class
    {
        if (model == null || conflicts == null) return;

        var type = typeof(TModel);

        // Obtener (o crear y cachear) el mapa de propiedades para este tipo.
        // Explicación:
        // - GetOrAdd: asegura creación atómica segura en multihilo.
        // - GetProperties(BindingFlags.Public | BindingFlags.Instance): trae propiedades públicas de instancia (incluye heredadas).
        // - Where(p => p.CanWrite): filtra propiedades que disponen de setter. Atención: CanWrite puede ser true aunque el setter sea non-public.
        //   Si se desea exclusivamente setter públicos, reemplazar con: p.SetMethod?.IsPublic == true
        // - ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase): crea un diccionario indexado por nombre de propiedad,
        //   usando comparación insensible a mayúsculas/minúsculas para facilitar búsquedas desde nombres recibidos externamente.
        var propertyMap = _propertyCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanWrite)
             // Excluir indexadores (propiedades con parámetros) para evitar colisiones/errores al SetValue
             .Where(p => p.GetIndexParameters().Length == 0)
             .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase)
        );

        foreach (var conflict in conflicts)
        {
            if (string.IsNullOrWhiteSpace(conflict.PropertyName)) continue;

            // Buscar PropertyInfo en el diccionario (búsqueda insensible a mayúsculas/minúsculas)
            if (propertyMap.TryGetValue(conflict.PropertyName, out var property))
            {
                try
                {
                    // Convertir el valor actual al tipo de la propiedad antes de asignar
                    var value = ConvertValue(conflict.OriginalValue, property.PropertyType);

                    // Intentar asignar; puede lanzar si el setter es no público o la conversión es incompatible.
                    property.SetValue(model, value);
                }
                catch (Exception ex)
                {
                    // Opcional: loguear, o ignorar silenciosamente para que un fallo en un campo no rompa todo.
                    // En entornos de producción es recomendable registrar el error con más contexto.
                    System.Diagnostics.Debug.WriteLine($"Error al asignar {conflict.PropertyName}: {ex.Message}");
                }
            }
            else
            {
                // Nota: si la propiedad no existe en el mapa, simplemente se ignora.
                // Podrías registrar un warning aquí si necesitas visibilidad de propiedades faltantes.
            }
        }
    }

    // Convierte de forma segura un objeto a un tipo objetivo (soporte básico para strings parseados).
    private static object? ConvertValue(object? value, Type targetType)
    {
        // Si el valor es null: devolver default para tipos valor o null para tipos referencia.
        if (value == null) return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        // Si el valor ya es asignable al tipo objetivo, devolver tal cual.
        if (targetType.IsAssignableFrom(value.GetType())) return value;

        // Manejo básico de conversión con intentos específicos antes de fallback.
        try
        {
            // Si el objetivo es string, usar ToString() del valor.
            if (targetType == typeof(string)) return value.ToString();

            if (value is string stringValue)
            {
                // Soporte para parseo desde string a tipos comunes.
                if (targetType == typeof(DateTime) && DateTime.TryParse(stringValue, out var dt))
                    return dt;
                if (targetType == typeof(bool) && bool.TryParse(stringValue, out var b))
                    return b;
                if (targetType == typeof(int) && int.TryParse(stringValue, out var i))
                    return i;
                if (targetType == typeof(decimal) && decimal.TryParse(stringValue, out var d))
                    return d;
                // Aquí puedes añadir más parseos específicos según los tipos de tu dominio.
            }

            // Fallback: intentar conversión estándar del framework.
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            // Si la conversión falla, devolver default para tipos valor o null para referencia.
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }
}
