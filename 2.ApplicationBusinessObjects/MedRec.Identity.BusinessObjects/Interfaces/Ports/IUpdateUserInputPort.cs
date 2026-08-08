using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IUpdateUserInputPort
{
    Task HandleAsync(UpdateUserDto dto, CancellationToken ct = default);
}
