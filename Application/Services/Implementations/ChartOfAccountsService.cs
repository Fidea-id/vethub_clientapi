using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class ChartOfAccountsService : GenericService<ChartOfAccounts, ChartOfAccountsRequest, ChartOfAccountsResponse, ChartOfAccountsFilter>, IChartOfAccountsService
    {
        public ChartOfAccountsService(IUnitOfWork unitOfWork, IGenericRepository<ChartOfAccounts, ChartOfAccountsFilter> repository, ICurrentUserService currentUser) : base(unitOfWork, repository, currentUser)
        {
        }
    }
}
