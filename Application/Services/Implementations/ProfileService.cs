using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Emails;
using Domain.Entities.Models;
using Domain.Entities.Responses;
using Domain.Interfaces;
using Domain.Utils;
using FluentEmail.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Application.Services.Implementations
{
    public class ProfileService : GenericService<Profile, Profile>, IProfileService
    {
        private readonly IEmailSender _emailsender;
        public ProfileService(IUnitOfWork unitOfWork, IGenericRepository<Profile> repository, IEmailSender emailsender)
        : base(unitOfWork, repository)
        {
            _emailsender = emailsender;
        }

        public async Task<UserProfileResponse> GetUserProfileByIdAsync(string dbName, int id)
        {
            var user = await _unitOfWork.ProfileRepository.GetById(dbName, id);
            var result = Mapping.Mapper.Map<UserProfileResponse>(user);
            return result;
        }

        public async Task TestSendEmail()
        {
            try
            {
                var data = new EmailSenderData
                {
                    Subject = "test",
                    To = "to@test.com",
                    CC = new List<Address>()
                    {
                        new Address{ EmailAddress = "cc@test.com", Name = "Test CC" }
                    },
                    EmailData = new TestTemplateData()
                    {
                        Date = DateTime.Now,
                        Name = "Test",
                    }
                };

                await _emailsender.Send(data);

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
