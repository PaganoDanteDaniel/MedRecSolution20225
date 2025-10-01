using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface ICreatePatientOutputPort : ICommonOutputPort
{
    bool Created { get; }
    Task Handle(Patient patient);    // Notifica éxito

}
