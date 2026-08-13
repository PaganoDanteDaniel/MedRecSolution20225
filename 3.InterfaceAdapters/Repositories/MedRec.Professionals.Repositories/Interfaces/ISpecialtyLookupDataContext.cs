using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.Repositories.Interfaces;
public interface ISpecialtyLookupDataContext
{
    Task<IReadOnlyList<SpecialtySummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
