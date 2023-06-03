using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Emails;
using Domain.Entities.Models;
using Domain.Entities.Requests;
using Domain.Entities.Responses;
using Domain.Interfaces;
using Domain.Utils;
using FluentEmail.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Application.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailSender _emailsender;
        public ProfileService(IUnitOfWork masterUOW, IEmailSender emailsender)
        {
            _uow = masterUOW;
            _emailsender = emailsender;
        }

        public async Task<UserProfileResponse> GetUserProfileByIdAsync(string dbName, int id)
        {
            var user = await _uow.ProfileRepository.GetById(dbName, id);
            var users = await _uow.ServicesRepository.GetAll(dbName);
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
