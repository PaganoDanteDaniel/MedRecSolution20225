using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class UpdateUserValidator
{
    public static IReadOnlyList<ValidationError> Validate(UpdateUserDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var nameValidation = Guard.Against(dto.FullName, nameof(dto.FullName))
            .NotNullOrEmpty("El nombre completo es obligatorio.");
        errors.AddRange(nameValidation.Errors);

        if (dto.RoleIds is null || dto.RoleIds.Count == 0)
            errors.Add(new ValidationError(nameof(dto.RoleIds), "El usuario debe tener al menos un rol asignado."));

        return errors;
    }
}
