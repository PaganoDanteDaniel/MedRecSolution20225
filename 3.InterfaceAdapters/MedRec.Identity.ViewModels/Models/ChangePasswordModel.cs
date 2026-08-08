using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.ViewModels.Models;
public class ChangePasswordModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public static explicit operator ChangePasswordDto(ChangePasswordModel model) =>
        new(model.CurrentPassword, model.NewPassword);
}
