using Domain.Entities.Emails;

namespace Domain.Interfaces
{
    public interface IEmailSender
    {
        Task Send(EmailSenderData data);
    }
}
