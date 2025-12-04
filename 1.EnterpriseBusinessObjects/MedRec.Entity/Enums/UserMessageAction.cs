namespace MedRec.Entity.Enums;
// En una carpeta como: MedRec.Application.Common / Enums / UI
public enum UserMessageAction
{
    /// <summary>
    /// No se requiere mensaje (éxito o error silencioso).
    /// </summary>
    None,

    /// <summary>
    /// Mostrar mensaje genérico de error (rojo).
    /// </summary>
    ShowError,

    /// <summary>
    /// Mostrar advertencia (amarillo/naranja), ej: "Ya existe".
    /// </summary>
    ShowWarning,

    /// <summary>
    /// Error de concurrencia: "Otro usuario modificó este registro".
    /// </summary>
    ShowConcurrencyMessage,

    /// <summary>
    /// Mostrar mensaje informativo (azul/verde), ej: "Guardado con éxito".
    /// </summary>
    ShowInfoMessage
}
