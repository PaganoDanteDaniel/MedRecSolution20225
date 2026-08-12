using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IProfessionalLookupDataContext
{
    Task<IReadOnlyList<ProfessionalSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
