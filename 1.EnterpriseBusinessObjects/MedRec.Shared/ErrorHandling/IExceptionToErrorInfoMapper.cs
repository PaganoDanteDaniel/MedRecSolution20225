using MedRec.Entity.DTOs;

namespace MedRec.Shared.ErrorHandling;
public interface IExceptionToErrorInfoMapper
{
    ErrorInfo Map(Exception ex);
}
