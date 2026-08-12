using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IGetProfessionalByIdOutputPort : IBaseOutputPort
{
    OperationResult<ProfessionalDto?> Result { get; }
    Task Handle(ProfessionalDto? professional, CancellationToken ct = default);
}
