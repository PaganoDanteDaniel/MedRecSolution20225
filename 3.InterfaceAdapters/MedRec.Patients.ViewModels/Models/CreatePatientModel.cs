using MedRec.Patients.BusinessObjects.Constraints;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.ViewModels.AttributeValidation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using static MedRec.Patients.ViewModels.AttributeValidation.EmailAttribute;

namespace MedRec.Patients.ViewModels.Models
{
    public class CreatePatientModel
    {
        #region Properties

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

        public string SelectedHealthCompanyName { get; set; }

        [MaxLength(500, ErrorMessage = "El mensaje de información debe contener como máximo 500 caracteres.")]
        public string InformationMessage { get; set; }

        #endregion

        #region Conversion
        public static explicit operator CreatePatientDto(CreatePatientModel model) =>
            new CreatePatientDto(
                firstName: model.FirstName?.ToUpper(),
                lastName: model.LastName?.ToUpper(),
                documentNumber: model.DocumentNumber,
                phoneNumber: model.PhoneNumber,
                dateOfBirth: model.DateOfBirth.Value,
                email: model.Email,
                insuranceHealthCompanyId: model.HealthInsuranceCompanyId,
                insuranceHealthMemberNumber: model.HealthInsuranceMemberNumber?.ToUpper(),
                insuranceHealthCard: model.HealthInsuranceCard?.ToUpper(),
                insuranceHealthPlan: model.HealthInsurancePlan?.ToUpper()
            );
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        #endregion
    }


}




//namespace MedRec.Patients.ViewModels.Models;
//public class CreatePatientModel
//{
//    #region Properties

//    [Required(Error = "El nombre es obligatorio.")]
//    [MinLength(2, Error = "El nombre debe tener al menos 2 caracteres.")]
//    [MaxLength(100, Error = "El nombre debe contener como máximo 100 caracteres.")]
//    public string FirstName { get; set; }

//    [Required(Error = "El apellido es obligatorio.")]
//    [MinLength(2, Error = "El apellido debe tener al menos 2 caracteres.")]
//    [MaxLength(100, Error = "El apellido debe contener como máximo 100 caracteres.")]
//    public string LastName { get; set; }

//    [Required(Error = "El número de documento es obligatorio.")]
//    [RegularExpression(@"^\d+$", Error = "El número de documento debe contener solo dígitos.")]
//    [MinLength(7, Error = "El número de documento debe tener al menos 7 dígitos.")]
//    [MaxLength(8, Error = "El número de documento debe tener como máximo 8 dígitos.")]
//    public string DocumentNumber { get; set; }

//    [Required(Error = "El teléfono es obligatorio.")]
//    [MinLength(6, Error = "El teléfono debe tener al menos 6 caracteres.")]
//    [MaxLength(14, Error = "El teléfono debe tener como máximo 14 caracteres.")]
//    public string PhoneNumber { get; set; }

//    [Required(Error = "La fecha de nacimiento es obligatoria.")]
//    [DataType(DataType.Date)]
//    [FutureDate(Error = "La fecha de nacimiento no puede ser en el futuro.")]
//    public DateTime DateOfBirth { get; set; }

//    [EmailAddress(Error = "El formato del email no es válido.")]
//    [MaxLength(256, Error = "El email debe contener como máximo 256 caracteres.")]
//    public string Email { get; set; }

//    public Guid? Id { get; set; }

//    [MaxLength(50, Error = "El número de afiliado debe contener como máximo 50 caracteres.")]
//    public string HealthInsuranceMemberNumber { get; set; }

//    [MaxLength(50, Error = "El número de tarjeta debe contener como máximo 50 caracteres.")]
//    public string HealthInsuranceCard { get; set; }

//    [MaxLength(100, Error = "El plan de salud debe contener como máximo 100 caracteres.")]
//    public string HealthInsurancePlan { get; set; }

//    [MaxLength(200, Error = "El nombre de la obra social debe contener como máximo 200 caracteres.")]
//    public string SelectedHealthCompanyName { get; set; }

//    [MaxLength(500, Error = "El mensaje de información debe contener como máximo 500 caracteres.")]
//    public string InformationMessage { get; set; }

//    #endregion



//    public static explicit operator CreatePatientDto(CreatePatientModel model)
//    {
//        return new CreatePatientDto(
//            firstName: model.FirstName,
//            lastName: model.LastName,
//            documentNumber: model.DocumentNumber,
//            phoneNumber: model.PhoneNumber,
//            dateOfBirth: model.DateOfBirth,
//            insuranceHealthCompanyId: model.Id,
//            insuranceHealthMemberNumber: model.HealthInsuranceMemberNumber,
//            insuranceHealthCard: model.HealthInsuranceCard,
//            insuranceHealthPlan: model.HealthInsurancePlan
//            );
//    }
//}

//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
//public class FutureDateAttribute : ValidationAttribute
//{
//    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
//    {
//        if (value is DateTime date)
//        {
//            if (date > DateTime.Now.Date)
//            {
//                return new ValidationResult(Error ?? "La fecha no puede ser en el futuro.");
//            }
//        }
//        return ValidationResult.Success;
//    }
//}