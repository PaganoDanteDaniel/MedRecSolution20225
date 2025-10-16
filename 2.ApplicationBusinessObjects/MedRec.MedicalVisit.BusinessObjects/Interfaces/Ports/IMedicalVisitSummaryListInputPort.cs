using MedRec.Entity.DTOs;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface IMedicalVisitSummaryListInputPort
{
    Task Handle(Guid patientId, PaginationDto paginationDto = default, CancellationToken cts = default);
}
