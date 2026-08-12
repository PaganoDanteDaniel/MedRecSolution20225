namespace MedRec.Professionals.BusinessObjects.DTOs;
public class SpecialtySummaryDto
{
    public SpecialtySummaryDto(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }
    public string Name { get; }
}
