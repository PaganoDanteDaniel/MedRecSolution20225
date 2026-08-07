using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IDoctorLookupRepository
{
    Task<IReadOnlyList<DoctorSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
