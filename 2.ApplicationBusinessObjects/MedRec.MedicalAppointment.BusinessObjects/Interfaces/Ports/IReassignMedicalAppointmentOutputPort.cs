using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface IReassignMedicalAppointmentOutputPort : IBaseOutputPort
{
    OperationResult<MedicalAppointmentDto> Result { get; }
    Task Handle(MedicalAppointmentView appointment, CancellationToken ct);
}
