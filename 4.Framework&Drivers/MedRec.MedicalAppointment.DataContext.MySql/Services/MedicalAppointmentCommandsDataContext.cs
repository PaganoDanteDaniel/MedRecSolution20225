using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalAppointments.Repositories.Interfaces;
using MedRec.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MedRec.MedicalAppointments.DataContext.MySql.Services;
internal class MedicalAppointmentCommandsDataContext(MedRecContext context)
    : IMedicalAppointmentCommandsDataContext
{
    public async Task CreateAsync(MedicalAppointment entity, CancellationToken ct) =>
            await context.MedicalAppointments.AddAsync(entity, ct);


    public async Task MoveAsync(MedicalAppointment entity, CancellationToken ct)
    {
        var existing = await context.MedicalAppointments
                            .FirstOrDefaultAsync(m => m.Id == entity.Id, ct);
        if (existing is null)
            throw new BusinessException(new ErrorInfo("Turno no encontrado.", ErrorCode.NotFound, entity.Id, 404));

        existing.DateTime = entity.DateTime;
        context.Entry(existing).Property("RowVersion").OriginalValue = entity.RowVersion;
    }

    public async Task ReassignAsync(MedicalAppointment entity, CancellationToken ct)
    {
        var existing = await context.MedicalAppointments
                            .FirstOrDefaultAsync(m => m.Id == entity.Id, ct);
        if (existing is null)
            throw new BusinessException(new ErrorInfo("Turno no encontrado.", ErrorCode.NotFound, entity.Id, 404));

        context.Entry(existing).CurrentValues.SetValues(entity);
        context.Entry(existing).Property("RowVersion").OriginalValue = entity.RowVersion;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var existing = await context.MedicalAppointments.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (existing is not null)
            context.MedicalAppointments.Remove(existing);
    }
}
