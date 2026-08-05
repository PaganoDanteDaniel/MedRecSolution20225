using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IAuthenticateUserInputPort
{
    Task HandleAsync(AuthenticateUserDto dto, CancellationToken ct = default);
}
