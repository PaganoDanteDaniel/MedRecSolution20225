using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

namespace MedRec.DynamicTemplates.Presenters.Implementation;

/// <summary>
/// Presenter for GetActiveSpecialties use case.
/// Inherits BaseOutputPort&lt;IEnumerable&lt;MedicalSpecialtyDto&gt;&gt; to expose OperationResult.
/// </summary>
public class GetActiveSpecialtiesPresenter : BaseOutputPort<IEnumerable<MedicalSpecialtyDto>>, IGetActiveSpecialtiesOutputPort
{
    public Task Handle(IEnumerable<MedicalSpecialtyDto> specialties)
    {
        Result = OperationResult<IEnumerable<MedicalSpecialtyDto>>.Ok(specialties);
        return Task.CompletedTask;
    }
}