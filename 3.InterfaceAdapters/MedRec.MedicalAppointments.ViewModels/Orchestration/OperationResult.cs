using MedRec.Entity.DTOs;
using MedRec.Validator.ValueObjects;

/*namespace MedRec.MedicalAppointments.ViewModels.Orchestration*/;


public sealed class OperationResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public ErrorInfo? Error { get; init; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; init; } = Array.Empty<ValidationError>();

    public bool HasError => Error is not null;
    public bool HasValidationErrors => ValidationErrors.Count > 0;

    private OperationResult() { }

    public static OperationResult<T> Ok(T? value) => new()
    {
        Success = true,
        Value = value
    };

    public static OperationResult<T> Fail(ErrorInfo error, IEnumerable<ValidationError>? validationErrors = null) => new()
    {
        Success = false,
        Error = error,
        ValidationErrors = (validationErrors ?? Enumerable.Empty<ValidationError>()).ToArray()
    };

    public static OperationResult<T> Unknown(string message = "Error desconocido.") =>
        Fail(new ErrorInfo(message));

    public static OperationResult<T> Cancelled(string message = "Operación cancelada por el usuario.") =>
        Fail(new ErrorInfo(message)); // Ajustar si agregas ErrorCode.Cancelled
}

public static class OperationResult
{
    public static OperationResult<T> Ok<T>(T? value) => OperationResult<T>.Ok(value);
    public static OperationResult<T> Fail<T>(ErrorInfo error, IEnumerable<ValidationError>? valErrs = null) =>
        OperationResult<T>.Fail(error, valErrs);
    public static OperationResult<T> Unknown<T>(string message = "Error desconocido.") =>
        OperationResult<T>.Unknown(message);
    public static OperationResult<T> Cancelled<T>(string message = "Operación cancelada por el usuario.") =>
        OperationResult<T>.Cancelled(message);
}