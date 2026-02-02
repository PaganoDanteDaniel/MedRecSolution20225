using MedRec.Entity.DTOs;
using MedRec.Validator.ValueObjects;

namespace MedRec.BusinessObjects.Interfaces;
public interface IBaseOutputPort
{
    Task ErrorAsync(ErrorInfo message);
    Task ValidationErrorsAsync(IEnumerable<ValidationError> errors);
}
