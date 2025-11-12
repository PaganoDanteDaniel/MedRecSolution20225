using MedRec.Entity.POCOEntities;

namespace MedRec.HealthInsurance.BusinessObjects.DTOs;
public class GetHealthInsuranceSummaryDto
{
    public GetHealthInsuranceSummaryDto(Guid id, string name, string acronym)
    {
        Id = id;
        Name = name;
        Acronym = acronym;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Acronym { get; }

    public static explicit operator GetHealthInsuranceSummaryDto(HealthInsuranceCompany entity)
    {
        return new GetHealthInsuranceSummaryDto(
            id: entity.Id,
            name: entity.Name,
            acronym: entity.Acronym);
    }
}
