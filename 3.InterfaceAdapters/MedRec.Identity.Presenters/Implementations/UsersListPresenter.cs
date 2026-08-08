using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class UsersListPresenter : BaseOutputPort<IReadOnlyList<UserSummaryDto>>, IUsersListOutputPort
{
    public Task Handle(IReadOnlyList<UserSummaryDto> users, CancellationToken ct = default)
    {
        Result = OperationResult<IReadOnlyList<UserSummaryDto>>.Ok(users);
        return Task.CompletedTask;
    }
}
