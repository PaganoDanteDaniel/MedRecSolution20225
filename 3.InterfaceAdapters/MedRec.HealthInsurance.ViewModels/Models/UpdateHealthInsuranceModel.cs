using MedRec.HealthInsurance.BusinessObjects.DTOs;

namespace MedRec.HealthInsurance.ViewModels.Models;
public class UpdateHealthInsuranceModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Acronym { get; set; }
    public byte[] RowVersion { get; set; }

    public string InformationMessage { get; set; }

    public static explicit operator UpdateHealthInsuranceDto(UpdateHealthInsuranceModel model)
    {
        return new UpdateHealthInsuranceDto
        (
            model.Id,
            model.Name?.ToUpperInvariant(),
            model.Acronym?.ToUpperInvariant(),
            model.RowVersion
        );
    }
}
