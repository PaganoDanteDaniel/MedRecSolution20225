using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class UserQueriesDataContextMySql(MedRecContext context) : IUserQueriesDataContext
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default)
    {
        return await (
            from ur in context.UserRoles
            join r in context.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId && !r.IsDeleted
            select r.Name
        ).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default)
    {
        return await (
            from ur in context.UserRoles
            join rp in context.RolePermissions on ur.RoleId equals rp.RoleId
            join p in context.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId && !p.IsDeleted
            select p.Code
        ).Distinct().ToListAsync(ct);
    }
}
