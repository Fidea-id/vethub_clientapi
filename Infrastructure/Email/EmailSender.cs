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
