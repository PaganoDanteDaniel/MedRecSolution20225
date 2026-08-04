using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalVisit.Presenters.Implementations;

internal class CreateMedicalVisitPresenter : BaseOutputPort<Guid>, ICreateMedicalVisitOutputPort
{
    public Task Handle(PatientMedicalVisit visit)
    {
        Result = OperationResult<Guid>.Ok(visit.Id);
        return Task.CompletedTask;
    }
}
