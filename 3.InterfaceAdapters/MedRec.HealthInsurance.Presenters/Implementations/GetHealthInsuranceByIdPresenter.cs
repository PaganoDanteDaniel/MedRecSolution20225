using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;

namespace MedRec.HealthInsurance.Presenters.Implementations;
internal class GetHealthInsuranceByIdPresenter :
    BaseOutputPort<GetHealthInsuranceDto>, IGetHealthInsuranceByIdOutputPort
{
    public Task Handle(HealthInsuranceCompany healthInsurance, CancellationToken ct = default)
    {
        var getHealthInsurance = new GetHealthInsuranceDto
        {
            Id = healthInsurance.Id,
            Name = healthInsurance.Name,
            Acronym = healthInsurance.Acronym,
            RowVersion = healthInsurance.RowVersion
        };

        Result = OperationResult<GetHealthInsuranceDto>.Ok(getHealthInsurance);

        return Task.CompletedTask;
    }
}
