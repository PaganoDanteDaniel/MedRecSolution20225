using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    // El parámetro "user" del PasswordHasher<TUser> de ASP.NET Core Identity no se usa
    // internamente por la implementación default: solo hace falta para la firma genérica.
    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
