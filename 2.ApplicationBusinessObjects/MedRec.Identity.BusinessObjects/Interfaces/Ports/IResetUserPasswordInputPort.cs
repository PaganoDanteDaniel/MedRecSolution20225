using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IResetUserPasswordInputPort
{
    Task HandleAsync(ResetUserPasswordDto dto, CancellationToken ct = default);
}
