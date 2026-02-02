using MedRec.Entity.DTOs;
using MedRec.Validator.ValueObjects;

namespace MedRec.BusinessObjects.Interfaces;
public interface ICommonOutputPort
{
    IEnumerable<ValidationError> ValidationErrors { get; }
    ErrorInfo ErrorMessage { get; }
    Task ValidationErrorsAsync(IEnumerable<ValidationError> errors); // Para errores de validación
    Task ErrorAsync(ErrorInfo? message); // Para errores generales
}
