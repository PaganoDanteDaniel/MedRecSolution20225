namespace MedRec.Identity.BusinessObjects.DTOs;
public class AuthResultDto
{
    public AuthResultDto(
        Guid userId,
        string email,
        string fullName,
        Guid? doctorId,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions,
        string token,
        DateTime expiresAtUtc)
    {
        UserId = userId;
        Email = email;
        FullName = fullName;
        DoctorId = doctorId;
        Roles = roles;
        Permissions = permissions;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; }
    public string Email { get; }
    public string FullName { get; }
    public Guid? DoctorId { get; }
    public IReadOnlyList<string> Roles { get; }
    public IReadOnlyList<string> Permissions { get; }
    public string Token { get; }
    public DateTime ExpiresAtUtc { get; }
}
