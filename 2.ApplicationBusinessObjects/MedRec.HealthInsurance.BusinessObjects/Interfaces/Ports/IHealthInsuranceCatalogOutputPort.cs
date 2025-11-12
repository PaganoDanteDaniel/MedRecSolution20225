using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IHealthInsuranceCatalogOutputPort : ICommonOutputPort
{
    int TotalRecords { get; }
    List<GetHealthInsuranceSummaryDto> HealthInsuranceCatalog { get; }
    Task Handle(IEnumerable<HealthInsuranceCompany> HealthInsuranceCatalog, int totalRecords, CancellationToken cts = default);

}
