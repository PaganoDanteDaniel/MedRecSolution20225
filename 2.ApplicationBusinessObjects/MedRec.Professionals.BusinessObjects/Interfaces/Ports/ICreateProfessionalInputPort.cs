using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface ICreateProfessionalInputPort
{
    Task HandleAsync(CreateProfessionalDto dto, CancellationToken ct = default);
}
