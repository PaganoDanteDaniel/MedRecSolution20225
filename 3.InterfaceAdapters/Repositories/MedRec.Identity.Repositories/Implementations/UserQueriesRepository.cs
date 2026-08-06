using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Interfaces;

namespace MedRec.Identity.Repositories.Implementations;
internal class UserQueriesRepository(IUserQueriesDataContext dataContext) : IUserQueriesRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        dataContext.GetByEmailAsync(email, ct);

    public Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default) =>
        dataContext.GetRoleNamesAsync(userId, ct);

    public Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default) =>
        dataContext.GetPermissionCodesAsync(userId, ct);
}
