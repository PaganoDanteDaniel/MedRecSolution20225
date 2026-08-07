using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Interfaces;

namespace MedRec.Identity.Repositories.Implementations;
internal class DoctorLookupRepository(IDoctorLookupDataContext dataContext) : IDoctorLookupRepository
{
    public Task<IReadOnlyList<DoctorSummaryDto>> ListActiveAsync(CancellationToken ct = default) =>
        dataContext.ListActiveAsync(ct);
}
