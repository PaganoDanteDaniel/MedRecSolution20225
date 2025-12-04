using MedRec.Entity.Enums;

namespace MedRec.Entity.DTOs;
public class ErrorInfo
{
    public ErrorInfo()
    {
        Message = "";
        Code = ErrorCode.None;
        Details = "";
        HttpStatusCode = 0;
        Timestamp = DateTime.UtcNow;
    }
    public ErrorInfo(
        string message,
        ErrorCode code = ErrorCode.None,
        object? details = null,
        int? httpStatusCode = null,
        DateTime? timestamp = null)
    {
        Message = message;
        Code = code;
        Details = details;
        HttpStatusCode = httpStatusCode;
        Timestamp = timestamp ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Mensaje legible para el usuario o UI.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Código interno de error (ej: DUPLICATE_KEY, CONCURRENCY_ERROR).
    /// </summary>
    public ErrorCode Code { get; }

    /// <summary>
    /// Datos adicionales útiles para debugging o mostrar en UI (ej: { Campo, Valor }).
    /// </summary>
    public object? Details { get; }

    /// <summary>
    /// Código HTTP sugerido (ej: 400, 409, 500).
    /// </summary>
    public int? HttpStatusCode { get; }

    /// <summary>
    /// Momento en que ocurrió el error (UTC).
    /// </summary>
    public DateTime Timestamp { get; }
}

