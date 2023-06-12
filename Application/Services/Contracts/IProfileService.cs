using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Entities.Requests;
using Domain.Entities.Responses;

namespace Application.Services.Contracts
{
    public interface IProfileService : IGenericService<Profile, ProfileRequest, Profile, ProfileFilter>
    {
        public Task<UserProfileResponse> GetUserProfileByIdAsync(string dbName, int id);
        public Task TestSendEmail();
    }
}
