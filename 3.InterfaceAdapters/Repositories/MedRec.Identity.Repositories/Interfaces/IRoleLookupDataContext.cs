using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IRoleLookupDataContext
{
    Task<IReadOnlyList<RoleSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
