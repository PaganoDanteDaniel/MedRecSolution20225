namespace MedRec.Identity.BusinessObjects.DTOs;
public class ResetUserPasswordDto
{
    public ResetUserPasswordDto(Guid userId, string temporaryPassword)
    {
        UserId = userId;
        TemporaryPassword = temporaryPassword;
    }

    public Guid UserId { get; }
    public string TemporaryPassword { get; }
}
