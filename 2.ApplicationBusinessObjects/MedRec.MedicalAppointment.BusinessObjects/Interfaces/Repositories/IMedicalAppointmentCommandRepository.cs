using MedRec.Entity.POCOEntities;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
public interface IMedicalAppointmentCommandRepository
{
    Task Create(MedicalAppointment entity, CancellationToken ct);
    Task Move(MedicalAppointment entity, CancellationToken ct);
    Task Reassign(MedicalAppointment entity, CancellationToken ct);
    Task Delete(Guid id, CancellationToken ct);
}
