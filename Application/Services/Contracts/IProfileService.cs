using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;
using System.Xml.Linq;

namespace Application.Services.Contracts
{
    public interface IProfileService : IGenericService<Profile, ProfileRequest, Profile, ProfileFilter>
    {
        public Task<UserProfileResponse> GetUserProfileByGlobalIdAsync(string dbName, int id);
        Task<Profile> GetOwnerProfile(string dbName);
        Task<UserProfileResponse> UpdateUserProfileByGlobalIdAsync(string dbName, ProfileRequest request, int id);
        public Task TestSendEmail();
    }
}
