using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface ICreateHealthInsuranceOutputPort : ICommonOutputPort
{
    bool Created { get; }
    Task Handle(HealthInsuranceCompany healthCompany, CancellationToken ct = default);
}
