using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
public interface IDeleteHealthInsuranceOutputPort : ICommonOutputPort
{
    bool IsDeleted { get; }
    Task Handle(HealthInsuranceCompany healthInsurance, CancellationToken ct = default);
}
