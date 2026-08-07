using MedRec.Entity.POCOEntities;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IUserCommandsRepository
{
    Task CreateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default);
    Task UpdateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default);
    Task SetActiveAsync(Guid userId, bool isActive, CancellationToken ct = default);
    Task SetPasswordAsync(Guid userId, string passwordHash, bool mustChangePassword, CancellationToken ct = default);
}
