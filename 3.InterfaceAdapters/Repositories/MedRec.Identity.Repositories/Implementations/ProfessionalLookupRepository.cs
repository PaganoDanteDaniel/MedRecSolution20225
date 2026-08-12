using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Interfaces;

namespace MedRec.Identity.Repositories.Implementations;
internal class ProfessionalLookupRepository(IProfessionalLookupDataContext dataContext) : IProfessionalLookupRepository
{
    public Task<IReadOnlyList<ProfessionalSummaryDto>> ListActiveAsync(CancellationToken ct = default) =>
        dataContext.ListActiveAsync(ct);
}
