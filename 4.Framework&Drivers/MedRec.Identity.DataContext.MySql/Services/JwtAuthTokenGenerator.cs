using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Shared.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class JwtAuthTokenGenerator(IOptions<Jwt> jwtOptions) : IAuthTokenGenerator
{
    public (string Token, DateTime ExpiresAtUtc) GenerateToken(
        Guid userId, string email, IReadOnlyList<string> roles, IReadOnlyList<string> permissions)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(claims: claims, expires: expiresAtUtc, signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
