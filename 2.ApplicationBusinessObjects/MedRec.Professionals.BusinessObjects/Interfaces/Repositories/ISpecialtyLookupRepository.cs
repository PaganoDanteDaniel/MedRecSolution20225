using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
public interface ISpecialtyLookupRepository
{
    Task<IReadOnlyList<SpecialtySummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
