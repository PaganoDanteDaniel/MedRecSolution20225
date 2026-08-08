using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class ChangePasswordPresenter : BaseOutputPort<bool>, IChangePasswordOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }

    public Task InvalidCurrentPassword()
    {
        Result = OperationResult<bool>.Fail(
            new ErrorInfo("La contraseña actual no es correcta.", ErrorCode.Forbidden, null, 401),
            UserMessageAction.ShowError);
        return Task.CompletedTask;
    }
}
