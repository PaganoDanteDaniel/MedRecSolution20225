using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IDoctorLookupDataContext
{
    Task<IReadOnlyList<DoctorSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
