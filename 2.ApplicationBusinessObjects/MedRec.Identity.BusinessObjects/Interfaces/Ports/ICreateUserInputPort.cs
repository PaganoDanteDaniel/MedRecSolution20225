using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface ICreateUserInputPort
{
    Task HandleAsync(CreateUserDto dto, CancellationToken ct = default);
}
