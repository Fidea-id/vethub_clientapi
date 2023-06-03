using Domain.Entities.Emails;
using Domain.Interfaces;
using FluentEmail.Core;

namespace Infrastructure.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IFluentEmail _email;
        private readonly string defaultpath = $"{Directory.GetCurrentDirectory()}";

        public EmailSender(IFluentEmail fluentEmail)
        {
            _email = fluentEmail;
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

            await email.SendAsync();
        }

        public string TemplatePath(string subject)
        {
            var path = string.Empty;
            if (subject == "test") path = Path.Combine(defaultpath, "wwwroot\\Template\\Email\\TestTemplate.cshtml");

            return path;
        }
    }
}
