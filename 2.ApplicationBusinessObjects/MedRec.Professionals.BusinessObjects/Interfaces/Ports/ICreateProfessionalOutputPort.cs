using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface ICreateProfessionalOutputPort : IBaseOutputPort
{
    OperationResult<Guid> Result { get; }
    Task Handle(Guid professionalId, CancellationToken ct = default);
}
