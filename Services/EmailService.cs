using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SistemaInventario.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, byte[] attachment = null, string attachmentName = null)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Inventario", "no-reply@inventario.com"));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                TextBody = body
            };

            if (attachment != null && !string.IsNullOrWhiteSpace(attachmentName))
            {
                builder.Attachments.Add(attachmentName, attachment, new ContentType("application", "pdf"));
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Mailtrap:Host"],
                int.Parse(_config["Mailtrap:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["Mailtrap:Username"],
                _config["Mailtrap:Password"]);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
