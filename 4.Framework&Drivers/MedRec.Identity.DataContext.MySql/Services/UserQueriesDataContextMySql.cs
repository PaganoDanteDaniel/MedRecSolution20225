using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class UserQueriesDataContextMySql(MedRecContext context) : IUserQueriesDataContext
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default)
    {
        return await (
            from ur in context.UserRoles
            join r in context.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId && !r.IsDeleted
            select r.Name
        ).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetRoleIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);
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

    public async Task<IReadOnlyList<UserSummaryDto>> ListWithRolesAsync(CancellationToken ct = default)
    {
        var users = await context.Users.Where(u => !u.IsDeleted).ToListAsync(ct);
        var result = new List<UserSummaryDto>();
        foreach (var user in users)
        {
            var roleNames = await GetRoleNamesAsync(user.Id, ct);
            result.Add(new UserSummaryDto(user.Id, user.Email, user.FullName, user.IsActive, roleNames));
        }
        return result;
    }
}
