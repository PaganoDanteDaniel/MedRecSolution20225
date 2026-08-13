using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;

namespace MedRec.Professionals.Presenters.Implementations;
internal class CreateProfessionalPresenter : BaseOutputPort<Guid>, ICreateProfessionalOutputPort
{
    public Task Handle(Guid professionalId, CancellationToken ct = default)
    {
        Result = OperationResult<Guid>.Ok(professionalId);
        return Task.CompletedTask;
    }
}
