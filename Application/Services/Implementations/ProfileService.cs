using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Emails;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;
using Domain.Interfaces;
using Domain.Interfaces.Clients;
using Domain.Utils;
using FluentEmail.Core.Models;

namespace Application.Services.Implementations
{
    public class ProfileService : GenericService<Profile, ProfileRequest, Profile, ProfileFilter>, IProfileService
    {
        private readonly IEmailSender _emailsender;
        public ProfileService(IUnitOfWork unitOfWork, IGenericRepository<Profile, ProfileFilter> repository, IEmailSender emailsender)
        : base(unitOfWork, repository)
        {
            _emailsender = emailsender;
        }

        public async Task<UserProfileResponse> GetUserProfileByGlobalIdAsync(string dbName, int id)
        {
            var user = await _unitOfWork.ProfileRepository.GetByGlobalId(dbName, id);
            var result = Mapping.Mapper.Map<UserProfileResponse>(user);
            return result;
        }

        public async Task<UserProfileResponse> UpdateUserProfileByGlobalIdAsync(string dbName, ProfileRequest request, int id)
        {

            //trim all string
            FormatUtil.TrimObjectProperties(request);
            //var entity = Mapping.Mapper.Map<Profile>(request); // cek dulu

            var checkedEntity = await _unitOfWork.ProfileRepository.GetByGlobalId(dbName, id);
            FormatUtil.ConvertUpdateObject<ProfileRequest, Profile>(request, checkedEntity);
            FormatUtil.SetDateBaseEntity<Profile>(checkedEntity, true);
            await _repository.Update(dbName, checkedEntity);

            var result = Mapping.Mapper.Map<UserProfileResponse>(checkedEntity);
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
