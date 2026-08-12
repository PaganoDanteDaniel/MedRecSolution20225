namespace MedRec.Identity.BusinessObjects.DTOs;
public class RoleSummaryDto
{
    public RoleSummaryDto(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }
    public string Name { get; }
}
