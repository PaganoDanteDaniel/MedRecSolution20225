using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MedRec.Patients.ViewModels.AttributeValidation;
public class EmailAttribute
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EmailValid : ValidationAttribute
    {
        private static readonly Regex _regex = new Regex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success; // null es válido, usá [Required] aparte si hace falta

            var email = value.ToString();

            if (_regex.IsMatch(email))
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage ??
                "El correo electrónico ingresado no tiene un formato válido.");
        }
    }
}
