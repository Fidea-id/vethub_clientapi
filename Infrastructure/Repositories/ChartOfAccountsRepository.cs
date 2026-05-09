using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ChartOfAccountsRepository : GenericRepository<ChartOfAccounts, ChartOfAccountsFilter>, IChartOfAccountsRepository
    {
        public ChartOfAccountsRepository(IDBFactory context) : base(context)
        {
        }
    }
}
