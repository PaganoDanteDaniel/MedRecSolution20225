namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IDeleteProfessionalInputPort
{
    Task HandleAsync(Guid id, CancellationToken ct = default);
}
