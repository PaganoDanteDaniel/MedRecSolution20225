using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class AuthenticateUserValidator
{
    public static IReadOnlyList<ValidationError> Validate(AuthenticateUserDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var emailValidation = Guard.Against(dto.Email, nameof(dto.Email))
            .NotNullOrEmpty("El email es obligatorio.")
            .InvalidEmail("El email no tiene un formato válido.");
        errors.AddRange(emailValidation.Errors);

        var passwordValidation = Guard.Against(dto.Password, nameof(dto.Password))
            .NotNullOrEmpty("La contraseña es obligatoria.");
        errors.AddRange(passwordValidation.Errors);

        return errors;
    }
}
