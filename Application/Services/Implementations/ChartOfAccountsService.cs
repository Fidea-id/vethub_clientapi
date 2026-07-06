using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Microsoft.Extensions.Configuration;

namespace Application.Services.Implementations
{
    public class ChartOfAccountsService : GenericService<ChartOfAccounts, ChartOfAccountsRequest, ChartOfAccountsResponse, ChartOfAccountsFilter>, IChartOfAccountsService
    {
        private readonly IConfiguration _configuration;

        public ChartOfAccountsService(IUnitOfWork unitOfWork, IGenericRepository<ChartOfAccounts, ChartOfAccountsFilter> repository, ICurrentUserService currentUser, IConfiguration configuration) : base(unitOfWork, repository, currentUser)
        {
            _configuration = configuration;
        }

        public override async Task DeleteAsync(int id, string? dbName)
        {
            var account = await _repository.GetById(dbName, id);
            if (account != null && IsLocked(account.Code))
            {
                throw new Exception($"Account {account.Code} is locked and cannot be deleted.");
            }
            await base.DeleteAsync(id, dbName);
        }

        public override async Task<ChartOfAccounts> UpdateAsync(int id, ChartOfAccountsRequest request, string? dbName)
        {
            var account = await _repository.GetById(dbName, id);
            if (account != null && IsLocked(account.Code) && account.Code != request.Code)
            {
                throw new Exception($"Account {account.Code} is locked. You cannot change its Code.");
            }
            return await base.UpdateAsync(id, request, dbName);
        }

        private bool IsLocked(string code)
        {
            var lockedAccounts = _configuration.GetSection("FinanceSettings:LockedAccounts").Get<string[]>();
            return lockedAccounts?.Contains(code) ?? false;
        }
    }
}
