using Domain.Entities.Emails;
using Domain.Interfaces;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IFluentEmail _email;
        private readonly ILogger<EmailSender> _logger;
        private readonly string defaultpath = $"{Directory.GetCurrentDirectory()}";

        public EmailSender(IFluentEmail fluentEmail, ILoggerFactory loggerFactory)
        {
            _email = fluentEmail;
            _logger = loggerFactory.CreateLogger<EmailSender>();
        }

        public async Task Send(EmailSenderData data)
        {
            var email = _email
                .To(data.To)
                .Subject(data.Subject)
                .CC(data.CC)
                .BCC(data.BCC)
                .Attach(data.Attachments)
                .UsingTemplateFromFile(TemplatePath(data.Subject), data.EmailData, true);

            var send = await email.SendAsync();
            _logger.LogInformation("Done sending with response:  " + JsonConvert.SerializeObject(send));
        }

        public string TemplatePath(string subject)
        {
            string path;
            if (subject == "Appointment Invoice")
            {
                path = Path.Combine(defaultpath, $"{Directory.GetCurrentDirectory()}/wwwroot/Template/Email/InvoiceTemplate.cshtml");
            }
            else
            {
                path = Path.Combine(defaultpath, $"{Directory.GetCurrentDirectory()}/wwwroot/Template/Email/TestTemplate.cshtml");
            }

            return path;
        }
    }
}
