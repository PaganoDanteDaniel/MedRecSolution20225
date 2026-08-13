using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;

namespace MedRec.Professionals.Presenters.Implementations;
internal class ListProfessionalsPresenter : BaseOutputPort<IReadOnlyList<ProfessionalDto>>, IListProfessionalsOutputPort
{
    public Task Handle(IReadOnlyList<ProfessionalDto> professionals, CancellationToken ct = default)
    {
        Result = OperationResult<IReadOnlyList<ProfessionalDto>>.Ok(professionals);
        return Task.CompletedTask;
    }
}
