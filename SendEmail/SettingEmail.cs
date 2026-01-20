using Microsoft.Extensions.Options;
using NotesApp.Models;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
namespace NotesApp.SendEmail
{
    public class SettingEmail
    {
        private readonly EmailSettings _email;
        public SettingEmail(IOptions<EmailSettings> emailSettings)
        {
            _email = emailSettings.Value;
        }
        public async Task SendEmail(string email,string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", _email.SmtpUser));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Проверка почты";
            message.Body = new TextPart("plain")
            {
                Text = body
            };
            using var client = new SmtpClient();

            await client.ConnectAsync(_email.SmtpServer, _email.SmtpPort,MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_email.SmtpUser, _email.SmtpPassword);
            await client.SendAsync(message);
            client.Disconnect(true);
        }
    }
}
