using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface IDeleteMedicalAppointmentOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(bool deleted, CancellationToken ct);
}
