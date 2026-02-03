using MedRec.Entity.POCOEntities;

namespace MedRec.MedicalAppointments.Repositories.Interfaces;
public interface IMedicalAppointmentCommandsDataContext
{
    Task CreateAsync(MedicalAppointment entity, CancellationToken ct);
    Task MoveAsync(MedicalAppointment entity, CancellationToken ct);
    Task ReassignAsync(MedicalAppointment entity, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
