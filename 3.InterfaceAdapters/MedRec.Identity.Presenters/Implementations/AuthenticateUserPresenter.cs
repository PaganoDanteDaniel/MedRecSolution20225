using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;

namespace MedRec.Identity.Presenters.Implementations;
internal class AuthenticateUserPresenter : BaseOutputPort<AuthResultDto>, IAuthenticateUserOutputPort
{
    public Task Handle(AuthResultDto result, CancellationToken ct = default)
    {
        Result = OperationResult<AuthResultDto>.Ok(result);
        return Task.CompletedTask;
    }

    public Task InvalidCredentials()
    {
        Result = OperationResult<AuthResultDto>.Fail(
            new ErrorInfo("Email o contraseña incorrectos.", ErrorCode.Forbidden, null, 401),
            UserMessageAction.ShowError);
        return Task.CompletedTask;
    }
}
