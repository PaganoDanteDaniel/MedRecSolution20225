namespace MedRec.Identity.BusinessObjects.DTOs;
public class UserSummaryDto
{
    public UserSummaryDto(Guid id, string email, string fullName, bool isActive, IReadOnlyList<string> roleNames)
    {
        Id = id;
        Email = email;
        FullName = fullName;
        IsActive = isActive;
        RoleNames = roleNames;
    }

    public Guid Id { get; }
    public string Email { get; }
    public string FullName { get; }
    public bool IsActive { get; }
    public IReadOnlyList<string> RoleNames { get; }
}
