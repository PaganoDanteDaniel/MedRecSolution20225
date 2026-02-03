using MedRec.Entity.POCOEntities;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalAppointments.Repositories.Interfaces;

namespace MedRec.MedicalAppointments.Repositories.Implementations;
internal class MedicalAppointmentCommandRepository(
    IMedicalAppointmentCommandsDataContext commandDataContext)
    : IMedicalAppointmentCommandRepository
{
    public async Task Create(MedicalAppointment entity, CancellationToken ct) =>
        await commandDataContext.CreateAsync(entity, ct);

    public async Task Delete(Guid id, CancellationToken ct) =>
        await commandDataContext.DeleteAsync(id, ct);

    public async Task Move(MedicalAppointment entity, CancellationToken ct) =>
        await commandDataContext.MoveAsync(entity, ct);

    public async Task Reassign(MedicalAppointment entity, CancellationToken ct) =>
        await commandDataContext.ReassignAsync(entity, ct);
}
