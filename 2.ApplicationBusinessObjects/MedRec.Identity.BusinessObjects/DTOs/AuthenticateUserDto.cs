namespace MedRec.Identity.BusinessObjects.DTOs;
public class AuthenticateUserDto
{
    public AuthenticateUserDto(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public string Email { get; }
    public string Password { get; }
}
