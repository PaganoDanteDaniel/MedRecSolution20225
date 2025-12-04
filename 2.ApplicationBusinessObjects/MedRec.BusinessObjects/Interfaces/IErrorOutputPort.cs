using MedRec.Entity.DTOs;

namespace MedRec.BusinessObjects.Interfaces;
public interface IErrorOutputPort
{
    ErrorInfo? ErrorMessage { get; }
    Task ErrorAsync(ErrorInfo message);
}
