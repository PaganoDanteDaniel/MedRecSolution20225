#nullable enable
using MedRec.BusinessObjects.Interfaces;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IUpdatePatientOutputPort : ICommonOutputPort
{
    bool UpdatedSuccessfully { get; }
    Task Handle(bool IsUpdated, CancellationToken cancellationToken = default);
}
