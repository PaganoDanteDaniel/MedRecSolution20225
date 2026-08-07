namespace MedRec.Identity.BusinessObjects.Interfaces.Services;
public interface IEmailNotificationService
{
    Task<bool> SendTemporaryPasswordAsync(string email, string fullName, string temporaryPassword, CancellationToken ct = default);
}
