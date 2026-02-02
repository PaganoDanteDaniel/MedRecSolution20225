namespace MedRec.HealthInsurance.BusinessObjects.DTOs;
public class CreateHealthInsuranceDto
{
    public CreateHealthInsuranceDto(string name, string acronym)
    {
        Name = name;
        Acronym = acronym;
    }
    public string Name { get; init; }
    public string Acronym { get; init; }
}
