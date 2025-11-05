using MedRec.MedicalAppointments.BusinessObjects.EntityView;

namespace MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
public interface IMedicalAppointmentQueriesRepository
{
    Task<MedicalAppointmentView> GetById(Guid id, CancellationToken ct);
    Task<IEnumerable<MedicalAppointmentView>> GetAllByDateRange((DateTime startDate, DateTime endDate) dateRange, CancellationToken ct);
    Task<bool> Exist(Guid id, CancellationToken ct);
}
