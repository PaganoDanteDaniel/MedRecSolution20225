using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalAppointments.Repositories.Interfaces;

namespace MedRec.MedicalAppointments.Repositories.Implementations;

internal class MedicalAppointmentQueriesRepository(
    IMedicalAppointmentQueriesDataContext context) : IMedicalAppointmentQueriesRepository
{
    public Task<bool> Exist(Guid id, CancellationToken ct) =>
        context.ExistAsync(id, ct);

    public Task<IEnumerable<MedicalAppointmentView>> GetAllByDateRange((DateTime startDate, DateTime endDate) dateRange, CancellationToken ct) =>
        context.GetAllByDateRangeAsync(dateRange, ct);

    public Task<MedicalAppointmentView> GetById(Guid id, CancellationToken ct) =>
        context.GetByIdAsync(id, ct);
}
