using MedRec.Entity.POCOEntities;
using MedRec.Entity.ValueObjects;
using MedRec.Patients.BusinessObjects.Constraints;
using MedRec.Patients.BusinessObjects.Resources;
using MedRec.Shared.Gruards;

namespace MedRec.Patients.BusinessObjects.Validators;
public static class CreatePatientValidator
{
    public static IReadOnlyList<ValidationError> Validate(Patient patient, bool validateId = false)
    {
        if (patient == null)
            throw new ArgumentNullException(nameof(patient));

        var errors = new List<ValidationError>();

        // Id
        if (validateId)
        {
            var idGuard = Guard.Against(patient.Id, nameof(patient.Id))
                .NotNullOrEmpty(PatientValidatorMessages.Id_Required);
            errors.AddRange(idGuard.Errors);
        }

        // Nombre
        // Nombre
        var firstNameValidation = Guard.Against(patient.FirstName, nameof(patient.FirstName))
            .NotNullOrEmpty(PatientValidatorMessages.FirstName_Required)
            .MaxLength(PatientConstraints.FirstNameMaxLength, string.Format(PatientValidatorMessages.FirstName_MaxLength, PatientConstraints.FirstNameMaxLength));
        errors.AddRange(firstNameValidation.Errors);

        // Apellido
        var lastNameValidation = Guard.Against(patient.LastName, nameof(patient.LastName))
            .NotNullOrEmpty(PatientValidatorMessages.LastName_Required)
            .MaxLength(PatientConstraints.LastNameMaxLength, string.Format(PatientValidatorMessages.LastName_MaxLength, PatientConstraints.LastNameMaxLength));
        errors.AddRange(lastNameValidation.Errors);

        // Documento
        var docValidation = Guard.Against(patient.DocumentNumber, nameof(patient.DocumentNumber))
            .NotNullOrEmpty(PatientValidatorMessages.Document_Required)
            .NonNumeric(PatientValidatorMessages.Document_Numeric)
            .MinLength(PatientConstraints.DocumentNumberMinLength, string.Format(PatientValidatorMessages.Document_MinLength, PatientConstraints.DocumentNumberMinLength))
            .MaxLength(PatientConstraints.DocumentNumberMaxLength, string.Format(PatientValidatorMessages.Document_MaxLength, PatientConstraints.DocumentNumberMaxLength));
        errors.AddRange(docValidation.Errors);

        // Teléfono
        var phoneValidation = Guard.Against(patient.PhoneNumber, nameof(patient.PhoneNumber))
            .NotNullOrEmpty(PatientValidatorMessages.Phone_Required)
            .MinLength(PatientConstraints.PhoneNumberMinLength, string.Format(PatientValidatorMessages.Phone_MinLength, PatientConstraints.PhoneNumberMinLength))
            .MaxLength(PatientConstraints.PhoneNumberMaxLength, string.Format(PatientValidatorMessages.Phone_MaxLength, PatientConstraints.PhoneNumberMaxLength));
        errors.AddRange(phoneValidation.Errors);

        // Email
        if (!string.IsNullOrWhiteSpace(patient.Email))
        {
            var emailValidation = Guard.Against(patient.Email, nameof(patient.Email))
                .MaxLength(PatientConstraints.EmailMaxLength, string.Format(PatientValidatorMessages.Email_Invalid, PatientConstraints.EmailMaxLength))
                .InvalidEmail(PatientValidatorMessages.Email_Invalid);
            errors.AddRange(emailValidation.Errors);
        }


        // Fecha de nacimiento
        var dobValidation = Guard.Against(patient.DateOfBirth, nameof(patient.DateOfBirth))
            .NotInFuture(PatientValidatorMessages.BirthDate_NotInFuture);
        errors.AddRange(dobValidation.Errors);

        return errors;
    }
}

