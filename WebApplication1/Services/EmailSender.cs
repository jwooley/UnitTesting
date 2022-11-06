using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;

namespace WebApplication1.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            client.PickupDirectoryLocation = @"C:\Temp";
            var msg = new MailMessage();
            msg.From = new MailAddress("donotreply@test123.com");
            msg.Subject = subject;
            msg.To.Add(new MailAddress(email));
            msg.Body = $"<html><body>{htmlMessage}</body></html>";
            msg.IsBodyHtml = true;
            return client.SendMailAsync(msg);
        }
    }
}
