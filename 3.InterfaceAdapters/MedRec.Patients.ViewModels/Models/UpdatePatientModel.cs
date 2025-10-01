using MedRec.Patients.BusinessObjects.Constraints;
using MedRec.Patients.ViewModels.AttributeValidation;
using System.ComponentModel.DataAnnotations;
using static MedRec.Patients.ViewModels.AttributeValidation.EmailAttribute;

namespace MedRec.Patients.ViewModels.Models;
internal class UpdatePatientModel
{
    [Required(ErrorMessage = "El identificador es obligatorio.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(PatientConstraints.FirstNameMaxLength,
            ErrorMessage = "El nombre debe contener como máximo {1} caracteres.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(PatientConstraints.LastNameMaxLength,
        ErrorMessage = "El apellido debe contener como máximo {1} caracteres.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "El número de documento debe contener solo dígitos.")]
    [MinLength(PatientConstraints.DocumentNumberMinLength,
        ErrorMessage = "El número de documento debe tener al menos {1} dígitos.")]
    [MaxLength(PatientConstraints.DocumentNumberMaxLength,
        ErrorMessage = "El número de documento debe tener como máximo {1} dígitos.")]
    public string DocumentNumber { get; set; }

    [MaxLength(PatientConstraints.AddressMaxLength, ErrorMessage = "La dirección no puede tener más de 100 caracteres.")]
    public string Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? CityId { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [MinLength(PatientConstraints.PhoneNumberMinLength,
        ErrorMessage = "El teléfono debe tener al menos {1} caracteres.")]
    [MaxLength(PatientConstraints.PhoneNumberMaxLength,
        ErrorMessage = "El teléfono debe tener como máximo {1} caracteres.")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [FutureDate(ErrorMessage = "La fecha de nacimiento no puede ser igual o mayor a la actual.")]
    public DateTime? DateOfBirth { get; set; }

    [EmailValid(ErrorMessage = "El formato del email no es válido.")]
    [MaxLength(PatientConstraints.EmailMaxLength,
        ErrorMessage = "El email debe contener como máximo {1} caracteres.")]
    public string Email { get; set; }
    public Guid? BiologicalSexId { get; set; }

    public Guid? HealthInsuranceCompanyId { get; set; }

    [MaxLength(PatientConstraints.HealthInsuranceMemberNumberMaxLength,
        ErrorMessage = "El número de afiliado debe contener como máximo {1} caracteres.")]
    public string HealthInsuranceMemberNumber { get; set; }


    [MaxLength(PatientConstraints.HealthInsuranceCardMaxLength,
        ErrorMessage = "El número de tarjeta debe contener como máximo {1} caracteres.")]
    public string HealthInsuranceCard { get; set; }

    [MaxLength(PatientConstraints.HealthInsurancePlanMaxLength,
        ErrorMessage = "El plan de salud debe contener como máximo {1} caracteres.")]
    public string HealthInsurancePlan { get; set; }


    public string SelectedCityName { get; set; }
    public string SelectedHealthCompanyName { get; set; }

    [MaxLength(500, ErrorMessage = "El mensaje de información debe contener como máximo 500 caracteres.")]
    public string InformationMessage { get; set; }




}
