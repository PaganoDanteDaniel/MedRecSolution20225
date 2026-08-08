using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IChangePasswordInputPort
{
    Task HandleAsync(ChangePasswordDto dto, CancellationToken ct = default);
}
