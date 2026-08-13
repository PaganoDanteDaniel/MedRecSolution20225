using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.Repositories.Interfaces;

namespace MedRec.Professionals.Repositories.Implementations;
internal class SpecialtyLookupRepository(ISpecialtyLookupDataContext dataContext) : ISpecialtyLookupRepository
{
    public Task<IReadOnlyList<SpecialtySummaryDto>> ListActiveAsync(CancellationToken ct = default) =>
        dataContext.ListActiveAsync(ct);
}
