using CreditTrack.Application.Interfaces;

using MailKit.Net.Smtp;
using MimeKit;



using MailKit.Security;

namespace CreditTrack.Infrastructure.RepoService
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpUser = "saheeraslam39@gmail.com";
        private readonly string _smtpPassword = "ompg nmgt kvgn xvsg";
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_smtpUser));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = body };

            using var smtp = new SmtpClient();

 
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpUser, _smtpPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
