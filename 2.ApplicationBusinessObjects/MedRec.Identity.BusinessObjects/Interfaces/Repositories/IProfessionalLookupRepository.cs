using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IProfessionalLookupRepository
{
    Task<IReadOnlyList<ProfessionalSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
