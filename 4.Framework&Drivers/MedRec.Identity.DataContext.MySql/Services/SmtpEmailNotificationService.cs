using System.Net;
using System.Net.Mail;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Shared.Security;
using Microsoft.Extensions.Options;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class SmtpEmailNotificationService(IOptions<EmailSettings> emailOptions) : IEmailNotificationService
{
    public async Task<bool> SendTemporaryPasswordAsync(string email, string fullName, string temporaryPassword, CancellationToken ct = default)
    {
        var settings = emailOptions.Value;
        if (string.IsNullOrEmpty(settings.SenderEmail) || string.IsNullOrEmpty(settings.SenderPassword))
            return false;

        try
        {
            using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                Credentials = new NetworkCredential(settings.SenderEmail, settings.SenderPassword),
                EnableSsl = true
            };

            using var message = new MailMessage(settings.SenderEmail, email)
            {
                Subject = "MedRec — Contraseña temporal",
                Body = $"Hola {fullName},\n\nSe generó una contraseña temporal para tu cuenta de MedRec: {temporaryPassword}\n\nVas a tener que cambiarla la primera vez que ingreses al sistema.\n\nSi no esperabas este email, contactá al administrador del sistema.",
                IsBodyHtml = false
            };

            await client.SendMailAsync(message, ct);
            return true;
        }
        catch (SmtpException)
        {
            return false;
        }
    }
}
