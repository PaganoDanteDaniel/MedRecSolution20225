namespace MedRec.HealthInsurance.BusinessObjects.DTOs;
public class GetHealthInsuranceDto
{
    public GetHealthInsuranceDto() { }
    public GetHealthInsuranceDto(Guid id, string name, string acronym, byte[] rowVersion)
    {
        Id = id;
        Name = name;
        Acronym = acronym;
        RowVersion = rowVersion;
    }

    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Acronym { get; init; }
    public byte[] RowVersion { get; init; }
}
