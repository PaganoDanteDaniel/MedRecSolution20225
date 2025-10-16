namespace MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
public interface IGetMedicalHistoryIdInputPort
{
    Task Handle(Guid patientId, CancellationToken cts = default);
}
