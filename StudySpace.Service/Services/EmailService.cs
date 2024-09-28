using MimeKit;
using StudySpace.Service.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Services
{
    public interface IEmailService
    {
        Task SendMailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _config;

        public EmailService(EmailConfiguration config)
        {
            _config = config;
        }

        public async Task SendMailAsync(string to, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_config.FromName, _config.FromEmail));
            emailMessage.To.Add(new MailboxAddress("Username", to));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await smtpClient.ConnectAsync(_config.SmtpServer, _config.SmtpPort, _config.UseSsl);
                await smtpClient.AuthenticateAsync(_config.SmtpUser, _config.SmtpPass);

                await smtpClient.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.ToString());
            }
            finally
            {
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
}
