using System;
using System.ComponentModel.DataAnnotations;

namespace MedRec.Patients.ViewModels.AttributeValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date && date > DateTime.Now.Date)
            {
                return new ValidationResult(ErrorMessage ?? "La fecha no puede ser en el futuro.", new[] { validationContext.MemberName });
            }
            return ValidationResult.Success;
        }
    }
}