using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;

namespace MedRec.Shared.Exceptions;

public class BusinessException : Exception
{
    public ErrorInfo Error { get; }

    public BusinessException(ErrorInfo errorInfo)
        : base(errorInfo.Message)
    {
        Error = errorInfo ?? throw new ArgumentNullException(nameof(errorInfo));
    }

    public BusinessException(string message, ErrorCode code = ErrorCode.None, object? details = null)
        : this(new ErrorInfo(message, code, details))
    {
    }
}
