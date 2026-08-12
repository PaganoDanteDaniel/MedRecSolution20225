using MedRec.Entity.Enums;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IListProfessionalsInputPort
{
    Task HandleAsync(ProfessionalType? typeFilter, CancellationToken ct = default);
}
