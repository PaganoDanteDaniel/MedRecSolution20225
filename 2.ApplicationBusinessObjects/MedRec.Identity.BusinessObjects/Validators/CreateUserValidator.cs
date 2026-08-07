using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class CreateUserValidator
{
    public static IReadOnlyList<ValidationError> Validate(CreateUserDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var emailValidation = Guard.Against(dto.Email, nameof(dto.Email))
            .NotNullOrEmpty("El email es obligatorio.")
            .InvalidEmail("El email no tiene un formato válido.");
        errors.AddRange(emailValidation.Errors);

        var nameValidation = Guard.Against(dto.FullName, nameof(dto.FullName))
            .NotNullOrEmpty("El nombre completo es obligatorio.");
        errors.AddRange(nameValidation.Errors);

        var passwordValidation = Guard.Against(dto.TemporaryPassword, nameof(dto.TemporaryPassword))
            .NotNullOrEmpty("La contraseña temporal es obligatoria.")
            .MinLength(8, "La contraseña temporal debe tener al menos 8 caracteres.");
        errors.AddRange(passwordValidation.Errors);

        if (dto.RoleIds is null || dto.RoleIds.Count == 0)
            errors.Add(new ValidationError(nameof(dto.RoleIds), "El usuario debe tener al menos un rol asignado."));

        return errors;
    }
}
