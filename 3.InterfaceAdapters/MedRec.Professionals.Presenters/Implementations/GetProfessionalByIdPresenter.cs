using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;

namespace MedRec.Professionals.Presenters.Implementations;
internal class GetProfessionalByIdPresenter : BaseOutputPort<ProfessionalDto?>, IGetProfessionalByIdOutputPort
{
    public Task Handle(ProfessionalDto? professional, CancellationToken ct = default)
    {
        Result = OperationResult<ProfessionalDto?>.Ok(professional);
        return Task.CompletedTask;
    }
}
