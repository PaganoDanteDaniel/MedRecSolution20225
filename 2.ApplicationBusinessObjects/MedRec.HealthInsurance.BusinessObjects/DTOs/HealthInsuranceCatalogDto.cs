using MedRec.Entity.POCOEntities;

namespace MedRec.HealthInsurance.BusinessObjects.DTOs;
public class HealthInsuranceCatalogDto
{
    public HealthInsuranceCatalogDto(Guid id, string name, string acronym)
    {
        Id = id;
        Name = name;
        Acronym = acronym;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Acronym { get; }

    public static explicit operator HealthInsuranceCatalogDto(HealthInsuranceCompany entity)
    {
        return new HealthInsuranceCatalogDto(
            id: entity.Id,
            name: entity.Name,
            acronym: entity.Acronym);
    }
}
