using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IHealthInsuranceCatalogOutputPort : IBaseOutputPort
{
    OperationResult<(IEnumerable<GetHealthInsuranceSummaryDto> healthInsurancesCatalog, int totalRecords)> Result { get; }
    Task Handle(IEnumerable<HealthInsuranceCompany> HealthInsuranceCatalog, int totalRecords, CancellationToken cts = default);

}
