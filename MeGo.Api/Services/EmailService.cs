using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace MeGo.Api.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config["Smtp:FromName"], _config["Smtp:FromEmail"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Your MEGO Verification Code";

            message.Body = new TextPart("plain")
            {
                Text = $"Your verification code is: {code}\n\n" +
                       "It will expire in 10 minutes.\n" +
                       "Please do not share this code with anyone.\n\n" +
                       "— The MEGO Team"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Smtp:Host"],
                int.Parse(_config["Smtp:Port"]),
                SecureSocketOptions.StartTls // ✅ Correct fix
            );
            await smtp.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
