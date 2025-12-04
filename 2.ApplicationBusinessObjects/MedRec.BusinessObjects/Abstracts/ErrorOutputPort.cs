using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.DTOs;

namespace MedRec.BusinessObjects.Abstracts;
public abstract class ErrorOutputPort : IErrorOutputPort
{
    private ErrorInfo? _errorMessage;
    public ErrorInfo? ErrorMessage => _errorMessage;
    public Task ErrorAsync(ErrorInfo message)
    {
        _errorMessage = message;
        return Task.CompletedTask;
    }
}
