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
    public async Task CreateAsync(MedicalAppointment entity, CancellationToken ct)
    {
        try
        {
            await context.MedicalAppointments.AddAsync(entity, ct);
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error al guardar el turno en la base de datos.",
                              ErrorCode.UpdateError,
                              ex.InnerException?.Message ?? ex.Message, 500));
        }
        catch (Exception ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error inesperado en la capa de persistencia.",
                              ErrorCode.Unknown,
                              ex.Message, 500));
        }
    }

    public async Task MoveAsync(MedicalAppointment entity, CancellationToken ct)
    {
        try
        {
            var existing = await context.MedicalAppointments
                                .FirstOrDefaultAsync(m => m.Id == entity.Id, ct);
            if (existing is null)
                throw new BusinessException(new ErrorInfo("Turno no encontrado.", ErrorCode.NotFound, entity.Id, 404));

            existing.DateTime = entity.DateTime;
            context.Entry(existing).Property("RowVersion").OriginalValue = entity.RowVersion;
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException(new ErrorInfo("Error al actualizar el turno.", ErrorCode.UpdateError, ex.InnerException?.Message ?? ex.Message, 500));
        }
        catch (Exception ex)
        {
            throw new BusinessException(new ErrorInfo("Error inesperado en DataContext.", ErrorCode.Unknown, ex.Message, 500));
        }
    }

    public async Task ReassignAsync(MedicalAppointment entity, CancellationToken ct)
    {
        try
        {
            var existing = await context.MedicalAppointments
                                .FirstOrDefaultAsync(m => m.Id == entity.Id, ct);
            if (existing is null)
                throw new BusinessException(new ErrorInfo("Turno no encontrado.", ErrorCode.NotFound, entity.Id, 404));

            context.Entry(existing).CurrentValues.SetValues(entity);
            context.Entry(existing).Property("RowVersion").OriginalValue = entity.RowVersion;
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException(new ErrorInfo("Error al reasignar el turno.", ErrorCode.UpdateError, ex.InnerException?.Message ?? ex.Message, 500));
        }
        catch (Exception ex)
        {
            throw new BusinessException(new ErrorInfo("Error inesperado en DataContext.", ErrorCode.Unknown, ex.Message, 500));
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var existing = await context.MedicalAppointments.FirstOrDefaultAsync(m => m.Id == id, ct);
            if (existing is not null)
                context.MedicalAppointments.Remove(existing);
            // si no existe, podrías decidir no lanzar error y que la operación sea idempotente
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException(new ErrorInfo("Error al eliminar el turno.", ErrorCode.UpdateError, ex.InnerException?.Message ?? ex.Message, 500));
        }
        catch (Exception ex)
        {
            throw new BusinessException(new ErrorInfo("Error inesperado en DataContext.", ErrorCode.Unknown, ex.Message, 500));
        }
    }
}
