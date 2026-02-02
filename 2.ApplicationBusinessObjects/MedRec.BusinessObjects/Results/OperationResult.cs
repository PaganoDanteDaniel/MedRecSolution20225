using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Validator.ValueObjects;

namespace MedRec.BusinessObjects.Results;

public sealed class OperationResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public ErrorInfo? Error { get; init; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; init; } = Array.Empty<ValidationError>();
    public UserMessageAction MessageAction { get; init; } = UserMessageAction.None;

    public bool HasError => Error is not null;
    public bool HasValidationErrors => ValidationErrors.Count > 0;

    private OperationResult() { }

    public static OperationResult<T> Ok(T? value) => new()
    {
        Success = true,
        Value = value
    };

    public static OperationResult<T> Fail(
     ErrorInfo error,
     UserMessageAction messageAction = UserMessageAction.ShowError,
     IEnumerable<ValidationError>? validationErrors = null) => new()
     {
         Success = false,
         Error = error,
         MessageAction = messageAction,
         ValidationErrors = (validationErrors ?? Enumerable.Empty<ValidationError>()).ToArray()
     };
    public static OperationResult<T> Fail(
        ErrorInfo error,
        IEnumerable<ValidationError>? validationErrors = null) => new()
        {
            Success = false,
            Error = error,
            ValidationErrors = (validationErrors ?? Enumerable.Empty<ValidationError>()).ToArray()
        };

    public static OperationResult<T> Validation(IEnumerable<ValidationError> errors) => new()
    {
        Success = false,
        ValidationErrors = errors.ToArray(),
        MessageAction = UserMessageAction.ShowError
    };
    public static OperationResult<T> Unknown(string message = "Error desconocido.") =>
        Fail(new ErrorInfo(message), UserMessageAction.ShowError);

    public static OperationResult<T> Cancelled(string message = "Operación cancelada por el usuario.") =>
        Fail(new ErrorInfo(message), UserMessageAction.ShowInfoMessage); // Ajustar si agregas ErrorCode.Cancelled
}

public static class OperationResult
{
    public static OperationResult<T> Ok<T>(T? value) => OperationResult<T>.Ok(value);
    public static OperationResult<T> Fail<T>(
    ErrorInfo error,
    UserMessageAction messageAction = UserMessageAction.ShowError,
    IEnumerable<ValidationError>? validationErrors = null) =>
        OperationResult<T>.Fail(error, messageAction, validationErrors);
    public static OperationResult<T> Fail<T>(
        ErrorInfo error,
        IEnumerable<ValidationError>? valErrs = null) =>
        OperationResult<T>.Fail(error, valErrs);
    public static OperationResult<T> Validation<T>(IEnumerable<ValidationError> errors) =>
        OperationResult<T>.Validation(errors);
    public static OperationResult<T> Unknown<T>(string message = "Error desconocido.") =>
        OperationResult<T>.Unknown(message);
    public static OperationResult<T> Cancelled<T>(string message = "Operación cancelada por el usuario.") =>
        OperationResult<T>.Cancelled(message);
}