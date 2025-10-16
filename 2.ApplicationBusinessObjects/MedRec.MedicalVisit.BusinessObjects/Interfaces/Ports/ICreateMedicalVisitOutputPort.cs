using MedRec.BusinessObjects.Interfaces;

namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface ICreateMedicalVisitOutputPort : ICommonOutputPort
{
    bool Created { get; }

    Task Handle();

}
