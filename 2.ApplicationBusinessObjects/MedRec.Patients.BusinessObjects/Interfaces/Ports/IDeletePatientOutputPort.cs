using MedRec.BusinessObjects.Interfaces;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IDeletePatientOutputPort : ICommonOutputPort
{
    bool IsDeleted { get; }
    Task Handle(bool successful, CancellationToken cancellationToken = default);
}
