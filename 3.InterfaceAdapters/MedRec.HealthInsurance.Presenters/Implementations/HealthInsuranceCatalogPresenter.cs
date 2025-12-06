using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;

namespace MedRec.HealthInsurance.Presenters.Implementations;
internal class HealthInsuranceCatalogPresenter :
    BaseOutputPort<(IEnumerable<GetHealthInsuranceSummaryDto>, int)>,
    IHealthInsuranceCatalogOutputPort
{
    public Task Handle(IEnumerable<HealthInsuranceCompany> healthInsuranceCatalog, int totalRecords, CancellationToken c)
    {
        ErrorAsync(null);

        var catalog = healthInsuranceCatalog.Select(x => new GetHealthInsuranceSummaryDto(
            id: x.Id,
            name: x.Name,
            acronym: x.Acronym)).ToList();

        var total = totalRecords;

        Result = OperationResult<(IEnumerable<GetHealthInsuranceSummaryDto>, int)>.Ok((catalog, total));

        return Task.CompletedTask;
    }
}
