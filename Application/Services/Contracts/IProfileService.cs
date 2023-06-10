using Domain.Entities.FIlters;
using Domain.Entities.Models;
using Domain.Entities.Responses;

namespace Application.Services.Contracts
{
    public interface IProfileService : IGenericService<Profile, Profile, Profile, ProfileFilter>
    {
        public Task<UserProfileResponse> GetUserProfileByIdAsync(string dbName, int id);
        public Task TestSendEmail();
    }
}
