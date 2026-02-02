using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.Patients.Presenters.Implementations;
internal class DeletePatientPresenter :
    BaseOutputPort<bool>,
    IDeletePatientOutputPort
{
    public Task Handle(CancellationToken cancellationToken = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
