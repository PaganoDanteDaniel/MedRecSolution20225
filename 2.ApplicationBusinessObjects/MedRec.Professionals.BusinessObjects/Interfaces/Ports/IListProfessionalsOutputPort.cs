using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IListProfessionalsOutputPort : IBaseOutputPort
{
    OperationResult<IReadOnlyList<ProfessionalDto>> Result { get; }
    Task Handle(IReadOnlyList<ProfessionalDto> professionals, CancellationToken ct = default);
}
