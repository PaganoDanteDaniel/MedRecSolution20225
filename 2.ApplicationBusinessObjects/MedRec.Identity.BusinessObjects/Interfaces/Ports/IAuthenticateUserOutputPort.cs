using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IAuthenticateUserOutputPort : IBaseOutputPort
{
    OperationResult<AuthResultDto> Result { get; }
    Task Handle(AuthResultDto result, CancellationToken ct = default);
    Task InvalidCredentials();
}
