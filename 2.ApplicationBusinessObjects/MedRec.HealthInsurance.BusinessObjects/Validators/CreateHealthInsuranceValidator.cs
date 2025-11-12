using MedRec.HealthInsurance.BusinessObjects.Constraints;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Resources;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.HealthInsurance.BusinessObjects.Validators;
public static class CreateHealthInsuranceValidator
{
    public static IReadOnlyList<ValidationError> Validate(CreateHealthInsuranceDto healthInsurance)
    {
        if (healthInsurance == null)
            throw new ArgumentNullException(nameof(healthInsurance));

        var errors = new List<ValidationError>();

        var nameValidation = Guard.Against(healthInsurance.Name, nameof(healthInsurance.Name))
            .NotNullOrEmpty(HealthInsuranceValidatorMessages.Name_Required)
            .MaxLength(HealthInsuranceConstraints.NameMaxLength,
            string.Format(HealthInsuranceValidatorMessages.Name_MaxLength, HealthInsuranceConstraints.NameMaxLength));
        errors.AddRange(nameValidation.Errors);

        if (!string.IsNullOrEmpty(healthInsurance.Acronym))
        {
            var acronym = Guard.Against(healthInsurance.Acronym, nameof(healthInsurance.Acronym))
                .MaxLength(HealthInsuranceConstraints.AcronymMaxLength,
                string.Format(HealthInsuranceValidatorMessages.Acronym_MaxLength, HealthInsuranceConstraints.AcronymMaxLength));
            errors.AddRange(acronym.Errors);
        }
        return errors;
    }

}
