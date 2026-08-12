namespace MedRec.Identity.BusinessObjects.DTOs;
public class ProfessionalSummaryDto
{
    public ProfessionalSummaryDto(Guid id, string fullName)
    {
        Id = id;
        FullName = fullName;
    }

    public Guid Id { get; }
    public string FullName { get; }
}
