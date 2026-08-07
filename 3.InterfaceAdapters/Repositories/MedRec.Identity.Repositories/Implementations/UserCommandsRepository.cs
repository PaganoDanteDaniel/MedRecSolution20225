using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Interfaces;

namespace MedRec.Identity.Repositories.Implementations;
internal class UserCommandsRepository(IUserCommandsDataContext dataContext) : IUserCommandsRepository
{
    public Task CreateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default) =>
        dataContext.CreateAsync(user, roleIds, ct);

    public Task UpdateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default) =>
        dataContext.UpdateAsync(user, roleIds, ct);

    public Task SetActiveAsync(Guid userId, bool isActive, CancellationToken ct = default) =>
        dataContext.SetActiveAsync(userId, isActive, ct);

    public Task SetPasswordAsync(Guid userId, string passwordHash, bool mustChangePassword, CancellationToken ct = default) =>
        dataContext.SetPasswordAsync(userId, passwordHash, mustChangePassword, ct);
}
