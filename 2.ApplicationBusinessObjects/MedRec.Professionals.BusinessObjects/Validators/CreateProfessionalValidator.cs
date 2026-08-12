using MedRec.Entity.Enums;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Professionals.BusinessObjects.Validators;
public static class CreateProfessionalValidator
{
    public static IReadOnlyList<ValidationError> Validate(CreateProfessionalDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        errors.AddRange(Guard.Against(dto.FirstName, nameof(dto.FirstName))
            .NotNullOrEmpty("El nombre es obligatorio.").Errors);

        errors.AddRange(Guard.Against(dto.LastName, nameof(dto.LastName))
            .NotNullOrEmpty("El apellido es obligatorio.").Errors);

        errors.AddRange(Guard.Against(dto.Email, nameof(dto.Email))
            .NotNullOrEmpty("El email es obligatorio.")
            .InvalidEmail("El email no tiene un formato válido.").Errors);

        errors.AddRange(Guard.Against(dto.Type, nameof(dto.Type)).IsDefined().Errors);

        if (dto.Type == ProfessionalType.Doctor || dto.Type == ProfessionalType.Nurse)
        {
            errors.AddRange(Guard.Against(dto.LicenseNumber ?? string.Empty, nameof(dto.LicenseNumber))
                .NotNullOrEmpty("La matrícula es obligatoria para este tipo de profesional.").Errors);
        }

        if (dto.Type == ProfessionalType.Doctor)
        {
            errors.AddRange(Guard.Against(dto.SpecialtyId ?? Guid.Empty, nameof(dto.SpecialtyId))
                .NotNullOrEmpty("La especialidad es obligatoria para médicos.").Errors);
        }

        return errors;
    }
}
