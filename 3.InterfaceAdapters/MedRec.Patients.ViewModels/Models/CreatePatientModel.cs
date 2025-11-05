using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.ViewModels.Models
{
    public class CreatePatientModel
    {
        #region Properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DocumentNumber { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string Email { get; set; }
        public Guid? HealthInsuranceCompanyId { get; set; }
        public string HealthInsuranceMemberNumber { get; set; }
        public string HealthInsuranceCard { get; set; }
        public string HealthInsurancePlan { get; set; }
        public string SelectedHealthCompanyName { get; set; }
        #endregion

        #region Conversion
        public static explicit operator CreatePatientDto(CreatePatientModel model)
        {
            if (model == null) return null;

            return new CreatePatientDto(
                firstName: model.FirstName?.ToUpperInvariant(),
                lastName: model.LastName?.ToUpperInvariant(),
                documentNumber: model.DocumentNumber,
                phoneNumber: model.PhoneNumber,
                dateOfBirth: model.DateOfBirth,
                email: model.Email,
                healthInsuranceId: model.HealthInsuranceCompanyId,
                healthInsuranceMemberNumber: model.HealthInsuranceMemberNumber?.ToUpperInvariant(),
                healthInsuranceCard: model.HealthInsuranceCard?.ToUpperInvariant(),
                healthInsurancePlan: model.HealthInsurancePlan?.ToUpperInvariant()
            );
        }
        #endregion
    }
}