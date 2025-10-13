using MedRec.BusinessObjects.Interfaces;

namespace MedRec.PatientMedicalVisit.BusinessObjects.Interfaces.Ports;
public interface ICreateMedicalVisitOutputPort : ICommonOutputPort
{
    bool Created { get; }

    Task Handle();

}
