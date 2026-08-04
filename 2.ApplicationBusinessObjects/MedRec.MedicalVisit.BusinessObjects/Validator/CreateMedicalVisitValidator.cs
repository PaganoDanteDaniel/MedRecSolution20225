namespace MedRec.MedicalVisit.BusinessObjects.Validator;

using global::MedRec.MedicalVisit.BusinessObjects.Constraints;
using global::MedRec.MedicalVisit.BusinessObjects.DTOs;
using global::MedRec.Validator.ValueObjects;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Resources;
using MedRec.Shared.Gruards;

public static class CreateMedicalVisitValidator
{
    public static IReadOnlyList<ValidationError> Validate(
        CreateMedicalVisitDto visit,
        IEnumerable<TemplateFieldDefinitionDto> fieldDefinitions)
    {
        if (visit == null)
            throw new ArgumentNullException(nameof(visit));

        var errors = new List<ValidationError>();

        var specialtyValidation = Guard.Against(visit.SpecialtyId, nameof(visit.SpecialtyId))
            .NotNullOrEmpty("La especialidad es obligatoria.");
        errors.AddRange(specialtyValidation.Errors);

        var dateValidation = Guard.Against(visit.VisitDate, nameof(visit.VisitDate))
            .NotInFuture(MedicalVisitValidatorMessages.VisitDate_NotInFuture);
        errors.AddRange(dateValidation.Errors);

        var reasonValidation = Guard.Against(visit.Reason, nameof(visit.Reason))
            .NotNullOrEmpty(MedicalVisitValidatorMessages.Reason_Required)
            .MaxLength(MedicalVisitConstraints.MaxLengthReason, MedicalVisitValidatorMessages.Reason_MaxLength);
        errors.AddRange(reasonValidation.Errors);

        if (!string.IsNullOrWhiteSpace(visit.Diagnosis))
        {
            var diagnosisValidation = Guard.Against(visit.Diagnosis, nameof(visit.Diagnosis))
                .MaxLength(MedicalVisitConstraints.MaxLengthDiagnosis,
                    string.Format(MedicalVisitValidatorMessages.Diagnosis_MaxLength, MedicalVisitConstraints.MaxLengthDiagnosis));
            errors.AddRange(diagnosisValidation.Errors);
        }

        if (!string.IsNullOrWhiteSpace(visit.Treatment))
        {
            var treatmentValidation = Guard.Against(visit.Treatment, nameof(visit.Treatment))
                .MaxLength(MedicalVisitConstraints.MaxLengthTreatment,
                    string.Format(MedicalVisitValidatorMessages.Treatment_MaxLength, MedicalVisitConstraints.MaxLengthTreatment));
            errors.AddRange(treatmentValidation.Errors);
        }

        var notesValidation = Guard.Against(visit.Notes, nameof(visit.Notes))
            .NotNullOrEmpty(MedicalVisitValidatorMessages.Note_Required)
            .MaxLength(MedicalVisitConstraints.MaxLengthNotes,
                string.Format(MedicalVisitValidatorMessages.Notes_MaxLength, MedicalVisitConstraints.MaxLengthNotes));
        errors.AddRange(notesValidation.Errors);

        var definitions = (fieldDefinitions ?? Enumerable.Empty<TemplateFieldDefinitionDto>())
            .ToDictionary(d => d.Id, d => d);

        foreach (var value in visit.DynamicFields ?? Enumerable.Empty<DynamicFieldValueDto>())
        {
            var fieldKey = $"DynamicFields[{value.FieldDefinitionId}]";

            if (!definitions.TryGetValue(value.FieldDefinitionId, out var definition))
            {
                errors.Add(new ValidationError(fieldKey, "El campo dinámico no está definido para esta especialidad."));
                continue;
            }

            var displayName = string.IsNullOrWhiteSpace(definition.FieldLabel)
                ? definition.FieldName
                : definition.FieldLabel;

            switch (definition.FieldType?.Trim().ToLowerInvariant())
            {
                case "number":
                case "decimal":
                    ValidateNumericField(value, definition, displayName, fieldKey, errors);
                    break;

                case "date":
                    ValidateDateField(value, definition, displayName, fieldKey, errors);
                    break;

                case "boolean":
                case "checkbox":
                    ValidateBooleanField(value, definition, displayName, fieldKey, errors);
                    break;

                default:
                    ValidateTextField(value, definition, displayName, fieldKey, errors);
                    break;
            }
        }

        return errors;
    }

    private static void ValidateNumericField(
        DynamicFieldValueDto value,
        TemplateFieldDefinitionDto definition,
        string displayName,
        string fieldKey,
        List<ValidationError> errors)
    {
        if (value.NumericValue is null)
        {
            if (definition.IsRequired)
            {
                errors.Add(new ValidationError(fieldKey, $"El campo '{displayName}' es obligatorio."));
            }

            return;
        }

        if (definition.MinimumValue.HasValue)
        {
            var minValidation = Guard.Against(value.NumericValue.Value, fieldKey)
                .GreaterThanOrEqualTo(definition.MinimumValue.Value,
                    $"El campo '{displayName}' debe ser mayor o igual que {definition.MinimumValue.Value}.");
            errors.AddRange(minValidation.Errors);
        }

        if (definition.MaximumValue.HasValue)
        {
            var maxValidation = Guard.Against(value.NumericValue.Value, fieldKey)
                .LessThanOrEqualTo(definition.MaximumValue.Value,
                    $"El campo '{displayName}' debe ser menor o igual que {definition.MaximumValue.Value}.");
            errors.AddRange(maxValidation.Errors);
        }
    }

    private static void ValidateDateField(
        DynamicFieldValueDto value,
        TemplateFieldDefinitionDto definition,
        string displayName,
        string fieldKey,
        List<ValidationError> errors)
    {
        if (value.DateValue is null && definition.IsRequired)
        {
            errors.Add(new ValidationError(fieldKey, $"El campo '{displayName}' es obligatorio."));
        }
    }

    private static void ValidateBooleanField(
        DynamicFieldValueDto value,
        TemplateFieldDefinitionDto definition,
        string displayName,
        string fieldKey,
        List<ValidationError> errors)
    {
        if (definition.IsRequired && value.BooleanValue is null)
        {
            errors.Add(new ValidationError(fieldKey, $"El campo '{displayName}' es obligatorio."));
        }
    }

    private static void ValidateTextField(
        DynamicFieldValueDto value,
        TemplateFieldDefinitionDto definition,
        string displayName,
        string fieldKey,
        List<ValidationError> errors)
    {
        if (!definition.IsRequired)
            return;

        var textValidation = Guard.Against(value.FieldValue ?? string.Empty, fieldKey)
            .NotNullOrEmpty($"El campo '{displayName}' es obligatorio.");
        errors.AddRange(textValidation.Errors);
    }
}
