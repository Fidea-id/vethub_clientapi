using Domain.Entities.Requests;
using Domain.Entities.Responses;

namespace Application.Services.Contracts
{
    public interface IProfileService
    {
        public Task<UserProfileResponse> GetUserProfileByIdAsync(string dbName, int id);
        public Task TestSendEmail();
    }
}
