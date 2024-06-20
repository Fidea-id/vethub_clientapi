using Application.Services.Contracts;
using Domain.Entities.Models.Masters;
using Domain.Interfaces.Clients;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _uow;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUnitOfWork uow) 
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            UserId = GetUserClientId();
        }

        public Task<int> UserId { get; }

        private async Task<int> GetUserClientId()
        {
            try
            {
                var globalId = Int32.Parse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("Id"));
                var dbName = _httpContextAccessor.HttpContext?.User?.FindFirstValue("Entity");
                var clientUser = await _uow.ProfileRepository.GetByGlobalId(dbName, globalId);
                return clientUser.Id;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }
    }
}
