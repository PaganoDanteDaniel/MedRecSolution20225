using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class ResetUserPasswordValidator
{
    public static IReadOnlyList<ValidationError> Validate(ResetUserPasswordDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var passwordValidation = Guard.Against(dto.TemporaryPassword, nameof(dto.TemporaryPassword))
            .NotNullOrEmpty("La contraseña temporal es obligatoria.")
            .MinLength(8, "La contraseña temporal debe tener al menos 8 caracteres.");
        errors.AddRange(passwordValidation.Errors);

        return errors;
    }
}
