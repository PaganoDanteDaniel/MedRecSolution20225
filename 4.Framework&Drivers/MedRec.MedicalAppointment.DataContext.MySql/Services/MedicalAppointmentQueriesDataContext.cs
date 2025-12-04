using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using MedRec.MedicalAppointments.Repositories.Interfaces;
using MedRec.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MedRec.MedicalAppointments.DataContext.MySql.Services;

internal class MedicalAppointmentQueriesDataContext(MedRecContext context) : IMedicalAppointmentQueriesDataContext
{
    public async Task<bool> ExistAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return await context.MedicalAppointments
            .AsNoTracking()
            .AnyAsync(m => m.Id == id && !m.IsDeleted, ct);
    }

    public async Task<IEnumerable<MedicalAppointmentView>> GetAllByDateRangeAsync((DateTime startDate, DateTime endDate) dateRange, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var (startDate, endDate) = dateRange;
        if (endDate < startDate)
            (startDate, endDate) = (endDate, startDate);

        var result = await context.MedicalAppointmentsView
            .AsNoTracking()
            .Where(m => !m.IsDeleted &&
                        m.AppointmentDateTime >= startDate &&
                        m.AppointmentDateTime <= endDate)
            .OrderBy(m => m.AppointmentDateTime)
            .ToListAsync(ct);

        return result;
    }

    public async Task<MedicalAppointmentView> GetByIdAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entity = await context.MedicalAppointmentsView
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted, ct);

        if (entity is null)
            throw new BusinessException(new
                ErrorInfo("Turno no encontrado.", ErrorCode.NotFound, id, 404));

        return entity;
    }
}
