using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalVisit.Presenters.Implementations;

internal class UpdateMedicalVisitPresenter : BaseOutputPort<bool>, IUpdateMedicalVisitOutputPort
{
    public Task Handle(bool updated, CancellationToken cd)
    {
        if (updated)
            Result = OperationResult<bool>.Ok(true);
        else
            Result = OperationResult<bool>.Fail(new ErrorInfo("No se pudo actualizar la visita médica."), UserMessageAction.ShowError);

        return Task.CompletedTask;
    }
}
