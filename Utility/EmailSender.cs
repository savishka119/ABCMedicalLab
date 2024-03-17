using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;

        public EmailSender(IOptions<EmailOptions> options)
        {
            _emailOptions = options.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSnder = new MimeMessage();
            emailSnder.Sender = MailboxAddress.Parse(_emailOptions.Mail);
            emailSnder.To.Add(MailboxAddress.Parse(email));
            emailSnder.Subject = subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = htmlMessage;
            emailSnder.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_emailOptions.Host, _emailOptions.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_emailOptions.Mail, _emailOptions.Password);
            await smtp.SendAsync(emailSnder);
            smtp.Disconnect(true);
           
        }
        
    }
}
