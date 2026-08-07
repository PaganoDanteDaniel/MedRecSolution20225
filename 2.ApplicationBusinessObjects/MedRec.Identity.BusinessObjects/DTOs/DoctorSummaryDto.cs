namespace MedRec.Identity.BusinessObjects.DTOs;
public class DoctorSummaryDto
{
    public DoctorSummaryDto(Guid id, string fullName)
    {
        Id = id;
        FullName = fullName;
    }

    public Guid Id { get; }
    public string FullName { get; }
}
