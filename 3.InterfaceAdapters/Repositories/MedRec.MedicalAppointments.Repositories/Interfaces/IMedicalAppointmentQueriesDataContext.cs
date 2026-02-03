using MedRec.MedicalAppointments.BusinessObjects.EntityView;

namespace MedRec.MedicalAppointments.Repositories.Interfaces;
public interface IMedicalAppointmentQueriesDataContext
{
    Task<MedicalAppointmentView> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<MedicalAppointmentView>> GetAllByDateRangeAsync((DateTime startDate, DateTime endDate) dateRange, CancellationToken ct);
    Task<bool> ExistAsync(Guid id, CancellationToken ct);
}
