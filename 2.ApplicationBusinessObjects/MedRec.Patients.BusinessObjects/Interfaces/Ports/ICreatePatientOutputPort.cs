using MedRec.BusinessObjects.Interfaces;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface ICreatePatientOutputPort : ICommonOutputPort
{
    bool Created { get; }
    Task Handle();    // Notifica éxito

}
