using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;

namespace MedRec.Identity.UseCases.Implementations;

public class UsersListInteractor(
    IUsersListOutputPort presenter,
    IUserQueriesRepository userQueriesRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext) : IUsersListInputPort
{
    public async Task HandleAsync(CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_View, ct);

        var users = await userQueriesRepository.ListWithRolesAsync(ct);
        await presenter.Handle(users, ct);
    }
}
