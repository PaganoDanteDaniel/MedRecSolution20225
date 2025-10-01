using MedRec.Entity.DTOs;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IPatientsListInputPort
{
    Task Handle(PaginationDto paginationDTO, CancellationToken cancellationToken = default);
}
