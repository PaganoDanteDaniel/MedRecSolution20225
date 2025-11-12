using MedRec.HealthInsurance.BusinessObjects.DTOs;

namespace MedRec.HealthInsurance.ViewModels.Models;
public class CreateHealthInsuranceModel
{
    private string _name;
    private string _acronym;
    private string _informationMessage;

    public string Name { get => _name; set => _name = value; }
    public string Acronym { get => _acronym; set => _acronym = value; }
    public string InformationMessage { get => _informationMessage; set => _informationMessage = value; }



    public static explicit operator CreateHealthInsuranceDto(CreateHealthInsuranceModel insuranceHealthCompany)
    {
        return new CreateHealthInsuranceDto
        (
            insuranceHealthCompany.Name?.ToUpperInvariant(),
            insuranceHealthCompany.Acronym?.ToUpperInvariant()
        );
    }
}
