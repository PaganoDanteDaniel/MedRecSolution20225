using MedRec.Entity.POCOEntities;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IUserQueriesRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default);
}
