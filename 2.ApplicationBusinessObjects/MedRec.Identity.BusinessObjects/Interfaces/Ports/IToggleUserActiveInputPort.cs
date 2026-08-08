using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IToggleUserActiveInputPort
{
    Task HandleAsync(ToggleUserActiveDto dto, CancellationToken ct = default);
}
