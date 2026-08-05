using MedRec.BusinessObjects.Interfaces;
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IAuthenticateUserOutputPort : ICommonOutputPort
{
    Task Handle(AuthResultDto result, CancellationToken ct = default);
    Task InvalidCredentials();
}
