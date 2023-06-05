using Domain.Entities.Models;
using Domain.Entities.Responses;

namespace Application.Services.Contracts
{
    public interface IProfileService: IGenericService<Profile, Profile>
    {
        public Task<UserProfileResponse> GetUserProfileByIdAsync(string dbName, int id);
        public Task TestSendEmail();
    }
}
