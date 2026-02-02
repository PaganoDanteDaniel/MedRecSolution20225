using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalAppointments.Presenters.Implementations;

internal class DeleteMedicalAppointmentPresenter : BaseOutputPort<bool>, IDeleteMedicalAppointmentOutputPort
{
    public Task Handle(bool deleted, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Result = OperationResult.Ok(deleted);

        return Task.CompletedTask;
    }
}
