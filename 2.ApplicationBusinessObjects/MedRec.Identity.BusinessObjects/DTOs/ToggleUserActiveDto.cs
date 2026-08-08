namespace MedRec.Identity.BusinessObjects.DTOs;
public class ToggleUserActiveDto
{
    public ToggleUserActiveDto(Guid userId, bool isActive)
    {
        UserId = userId;
        IsActive = isActive;
    }

    public Guid UserId { get; }
    public bool IsActive { get; }
}
