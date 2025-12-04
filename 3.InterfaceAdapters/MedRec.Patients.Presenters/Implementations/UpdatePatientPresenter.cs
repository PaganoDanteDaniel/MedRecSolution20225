using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.Patients.Presenters.Implementations;
internal class UpdatePatientPresenter :
    BaseOutputPort<bool>,
    IUpdatePatientOutputPort
{

    public Task Handle(CancellationToken cancellationToken = default)
    {
        Result = OperationResult.Ok(true);
        return Task.CompletedTask;
    }
}
