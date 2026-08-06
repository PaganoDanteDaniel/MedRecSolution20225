using MedRec.BusinessObjects.Interfaces;
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IAuthenticateUserOutputPort : IBaseOutputPort
{
    Task Handle(AuthResultDto result, CancellationToken ct = default);
    Task InvalidCredentials();
}
