using MedRec.Entity.POCOEntities;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IUserCommandsDataContext
{
    Task CreateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default);
    Task UpdateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default);
    Task SetActiveAsync(Guid userId, bool isActive, CancellationToken ct = default);
    Task SetPasswordAsync(Guid userId, string passwordHash, bool mustChangePassword, CancellationToken ct = default);
}
