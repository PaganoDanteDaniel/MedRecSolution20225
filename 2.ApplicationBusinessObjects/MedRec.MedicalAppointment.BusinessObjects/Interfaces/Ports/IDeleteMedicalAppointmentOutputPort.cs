using MedRec.BusinessObjects.Interfaces;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
public interface IDeleteMedicalAppointmentOutputPort : ICommonOutputPort
{
    bool IsDeleted { get; }
    Task Handle(bool deleted, CancellationToken ct);
}
