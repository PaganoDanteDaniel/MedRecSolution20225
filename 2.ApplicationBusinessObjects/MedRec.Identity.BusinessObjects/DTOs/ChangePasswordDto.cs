namespace MedRec.Identity.BusinessObjects.DTOs;
public class ChangePasswordDto
{
    public ChangePasswordDto(string currentPassword, string newPassword)
    {
        CurrentPassword = currentPassword;
        NewPassword = newPassword;
    }

    public string CurrentPassword { get; }
    public string NewPassword { get; }
}
