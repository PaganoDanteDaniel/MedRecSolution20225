using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class ChangePasswordValidator
{
    public static IReadOnlyList<ValidationError> Validate(ChangePasswordDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var currentValidation = Guard.Against(dto.CurrentPassword, nameof(dto.CurrentPassword))
            .NotNullOrEmpty("Debe ingresar su contraseña actual.");
        errors.AddRange(currentValidation.Errors);

        var newValidation = Guard.Against(dto.NewPassword, nameof(dto.NewPassword))
            .NotNullOrEmpty("La nueva contraseña es obligatoria.")
            .MinLength(8, "La nueva contraseña debe tener al menos 8 caracteres.");
        errors.AddRange(newValidation.Errors);

        return errors;
    }
}
