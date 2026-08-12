namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IGetProfessionalByIdInputPort
{
    Task HandleAsync(Guid id, CancellationToken ct = default);
}
