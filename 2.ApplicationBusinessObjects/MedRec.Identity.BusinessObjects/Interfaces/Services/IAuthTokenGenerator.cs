namespace MedRec.Identity.BusinessObjects.Interfaces.Services;
public interface IAuthTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(
        Guid userId, string email, IReadOnlyList<string> roles, IReadOnlyList<string> permissions);
}
