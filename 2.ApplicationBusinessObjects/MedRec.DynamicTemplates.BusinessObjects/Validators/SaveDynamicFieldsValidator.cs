using MedRec.DynamicTemplates.BusinessObjects.Constraints;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Resources;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.DynamicTemplates.BusinessObjects.Validators;

public static class SaveDynamicFieldsValidator
{
    public static IReadOnlyList<ValidationError> Validate(SaveDynamicFieldsDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        // Validar PatientMedicalVisitId
        var visitIdValidation = Guard.Against(dto.PatientMedicalVisitId, nameof(dto.PatientMedicalVisitId))
            .NotNullOrEmpty(DynamicTemplatesValidatorMessages.VisitId_Required);
        errors.AddRange(visitIdValidation.Errors);

        // Validar que existan campos
        if (dto.Fields == null || !dto.Fields.Any())
        {
            errors.Add(new ValidationError(nameof(dto.Fields),
                DynamicTemplatesValidatorMessages.Fields_Required));
            return errors;
        }

        // Validar cada campo
        foreach (var field in dto.Fields)
        {
            var fieldErrors = ValidateFieldValue(field);
            errors.AddRange(fieldErrors);
        }

        return errors;
    }

    private static IReadOnlyList<ValidationError> ValidateFieldValue(DynamicFieldValueDto field)
    {
        var errors = new List<ValidationError>();

        // Validar FieldDefinitionId
        var fieldIdValidation = Guard.Against(field.FieldDefinitionId, nameof(field.FieldDefinitionId))
            .NotNullOrEmpty(DynamicTemplatesValidatorMessages.FieldDefinitionId_Required);
        errors.AddRange(fieldIdValidation.Errors);

        // Validar longitud del valor si existe
        if (!string.IsNullOrEmpty(field.FieldValue))
        {
            var fieldValueValidation = Guard.Against(field.FieldValue, nameof(field.FieldValue))
                .MaxLength(DynamicTemplatesConstraints.FieldValueMaxLength,
                    string.Format(DynamicTemplatesValidatorMessages.FieldValue_MaxLength,
                        DynamicTemplatesConstraints.FieldValueMaxLength));
            errors.AddRange(fieldValueValidation.Errors);
        }

        return errors;
    }
}