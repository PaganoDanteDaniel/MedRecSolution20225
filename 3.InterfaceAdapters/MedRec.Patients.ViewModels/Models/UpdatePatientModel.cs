using MedRec.Entity.Enums;
using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.ViewModels.Models;
public class UpdatePatientModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DocumentNumber { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; }
    public BiologicalSex BiologicalSexId { get; set; }
    public Guid? HealthInsuranceCompanyId { get; set; }
    public string HealthInsuranceMemberNumber { get; set; }
    public string HealthInsuranceCard { get; set; }
    public string HealthInsurancePlan { get; set; }
    public string SelectedCityName { get; set; }
    public string SelectedHealthCompanyName { get; set; }
    public byte[] RowVersion { get; set; }


    public static explicit operator UpdatePatientDto(UpdatePatientModel model)
    {
        if (model == null) return null;

        return new UpdatePatientDto(
                    model.Id,
                    model.FirstName,
                    model.LastName,
                    model.DocumentNumber,
                    model.DateOfBirth,
                    model.Address,
                    model.PhoneNumber,
                    model.Email,
                    model.BiologicalSexId,
                    model.HealthInsuranceCompanyId,
                    model.HealthInsuranceMemberNumber,
                    model.HealthInsuranceCard,
                    model.HealthInsurancePlan,
                    model.RowVersion);

    }
}
