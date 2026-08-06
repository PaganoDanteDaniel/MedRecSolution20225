using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.ViewModels.Models;
public class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public static explicit operator AuthenticateUserDto(LoginModel model) =>
        new(model.Email, model.Password);
}
