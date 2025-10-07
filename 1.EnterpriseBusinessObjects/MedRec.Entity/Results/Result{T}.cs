using MedRec.Entity.DTOs;

namespace MedRec.Entity.Results;

public sealed record Result<T>
{
    public bool IsSuccess { get; init; }
    public T Value { get; init; }
    public ErrorInfo? Error { get; init; }
    public int RowAffected { get; init; }

    public Result(bool isSuccess, T value, int rowAffected, ErrorInfo? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        RowAffected = rowAffected;
        Error = error;
    }

    public static Result<T> Ok(T value, int rowAffected = 0) => new(true, value, rowAffected, null);
    public static Result<T> Fail(ErrorInfo error)
    {
        _ = error ?? throw new ArgumentNullException(nameof(error));
        return new(false, default!, 0, error);
    }
    public T EnsureSuccess()
    {
        if (!IsSuccess)
            throw new InvalidOperationException($"Operación fallida: {Error?.Message ?? "Error desconocido"}");
        return Value;
    }
}
// Conversión implícita desde T (opcional, pero útil)
//public static implicit operator Result<T>(T value) => Ok(value);


//public static Result<T> Ok(T? value = default) => new(true, value, null);
//public static Result<T> Fail(ErrorInfo errorMessage) => new(false, default, errorMessage);  