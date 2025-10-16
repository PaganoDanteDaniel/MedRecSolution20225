namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface IGetMedicalVisitInputPort
{
    Task Handle(Guid medicalVisitId, CancellationToken cts = default);
}
