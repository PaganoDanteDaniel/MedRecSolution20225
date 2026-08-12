using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IUpdateProfessionalInputPort
{
    Task HandleAsync(UpdateProfessionalDto dto, CancellationToken ct = default);
}
