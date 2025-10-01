using MedRec.Entity.DTOs;

namespace MedRec.Entity.Results;

public sealed record Result<T>
{
    public bool IsSuccess { get; init; }
    public T Value { get; init; }
    public ErrorInfo? Error { get; init; }

    public Result(bool isSuccess, T value, ErrorInfo? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(ErrorInfo error)
    {
        _ = error ?? throw new ArgumentNullException(nameof(error));
        return new(false, default!, error);
    }
    public T EnsureSuccess()
    {
        if (!IsSuccess)
            throw new InvalidOperationException($"Operación fallida: {Error?.Message ?? "Error desconocido"}");
        return Value;
    }

    // Conversión implícita desde T (opcional, pero útil)
    public static implicit operator Result<T>(T value) => Ok(value);


    //public static Result<T> Ok(T? value = default) => new(true, value, null);
    //public static Result<T> Fail(ErrorInfo errorMessage) => new(false, default, errorMessage);    
}
