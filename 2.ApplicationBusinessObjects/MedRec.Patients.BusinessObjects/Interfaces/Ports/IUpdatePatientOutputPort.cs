#nullable enable
using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IUpdatePatientOutputPort : ICommonOutputPort
{
    bool IsSuccessful { get; }
    Task Handle(Patient patient, CancellationToken cancellationToken = default);
}
