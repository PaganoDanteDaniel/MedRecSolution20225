using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Validator.ValueObjects;

namespace MedRec.BusinessObjects.Abstracts;
public abstract class BaseOutputPort<T> : IBaseOutputPort
{
    public OperationResult<T> Result { get; protected set; } = OperationResult.Ok<T>(default!);

    public virtual Task ErrorAsync(ErrorInfo? message)
    {
        if (message is null)
        {
            message = new ErrorInfo();
            Result = OperationResult.Fail<T>(message, UserMessageAction.None);
            return Task.CompletedTask;
        }

        var action = message.Code switch
        {
            ErrorCode.DuplicateKey => UserMessageAction.ShowWarning,
            ErrorCode.ConcurrencyError => UserMessageAction.ShowConcurrencyMessage,
            ErrorCode.DatabaseError => UserMessageAction.ShowError,
            ErrorCode.NotFound => UserMessageAction.ShowError,
            ErrorCode.Forbidden => UserMessageAction.ShowError,
            ErrorCode.Cancelled => UserMessageAction.ShowInfoMessage,
            _ => UserMessageAction.ShowError
        };

        Result = OperationResult.Fail<T>(message, action);
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        Result = OperationResult.Validation<T>(errors);
        return Task.CompletedTask;
    }
}
