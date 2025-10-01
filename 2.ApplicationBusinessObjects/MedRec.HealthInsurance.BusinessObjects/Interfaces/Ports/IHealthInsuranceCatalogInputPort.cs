using MedRec.Entity.DTOs;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IHealthInsuranceCatalogInputPort
{
    Task Handle(PaginationDto pagination, CancellationToken cancellationToken);
}
