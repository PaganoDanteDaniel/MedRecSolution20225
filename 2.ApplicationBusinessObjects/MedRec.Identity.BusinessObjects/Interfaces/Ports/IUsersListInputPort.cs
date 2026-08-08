namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IUsersListInputPort
{
    Task HandleAsync(CancellationToken ct = default);
}
