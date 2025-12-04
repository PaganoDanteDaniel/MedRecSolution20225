using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.Patients.Presenters.Implementations;
internal class CreatePatientPresenter :
    BaseOutputPort<bool>,
    ICreatePatientOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
