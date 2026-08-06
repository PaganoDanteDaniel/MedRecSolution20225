using MedRec.Entity.POCOEntities;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IUserQueriesDataContext
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default);
}
