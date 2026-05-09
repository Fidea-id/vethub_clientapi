using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IChartOfAccountsService : IGenericService<ChartOfAccounts, ChartOfAccountsRequest, ChartOfAccountsResponse, ChartOfAccountsFilter>
    {
    }
}
