using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IUsersListOutputPort : IBaseOutputPort
{
    OperationResult<IReadOnlyList<UserSummaryDto>> Result { get; }
    Task Handle(IReadOnlyList<UserSummaryDto> users, CancellationToken ct = default);
}
