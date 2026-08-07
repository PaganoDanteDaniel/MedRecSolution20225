using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class UserCommandsDataContextMySql(MedRecContext context) : IUserCommandsDataContext
{
    public async Task CreateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
        foreach (var roleId in roleIds)
            await context.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = roleId }, ct);
    }

    public async Task UpdateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default)
    {
        var tracked = context.ChangeTracker.Entries<User>().FirstOrDefault(e => e.Entity.Id == user.Id);
        if (tracked is not null)
            tracked.State = EntityState.Detached;

        context.Attach(user);
        var entry = context.Entry(user);
        entry.State = EntityState.Modified;
        entry.Property(nameof(User.RowVersion)).OriginalValue = user.RowVersion;

        var existingRoles = await context.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync(ct);
        context.UserRoles.RemoveRange(existingRoles);
        foreach (var roleId in roleIds)
            await context.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = roleId }, ct);
    }

    public async Task SetActiveAsync(Guid userId, bool isActive, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;
        user.IsActive = isActive;
    }

    public async Task SetPasswordAsync(Guid userId, string passwordHash, bool mustChangePassword, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;
        user.PasswordHash = passwordHash;
        user.MustChangePassword = mustChangePassword;
    }
}
