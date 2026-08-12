using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IRoleLookupRepository
{
    Task<IReadOnlyList<RoleSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
