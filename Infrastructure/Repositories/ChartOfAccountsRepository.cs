using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<Dictionary<int, double>> GetAccountBalancesAsync(string dbName)
        {
            const string sql = @"
                SELECT 
                    ChartOfAccountId, 
                    SUM(Debit) AS TotalDebit, 
                    SUM(Credit) AS TotalCredit
                FROM JournalEntries
                WHERE IsActive = 1
                GROUP BY ChartOfAccountId";

            using (var db = _dbFactory.GetDbConnection(dbName))
            {
                var raw = await db.QueryAsync<AccountBalanceQueryModel>(sql);
                return raw.ToDictionary(
                    x => x.ChartOfAccountId,
                    x => x.TotalDebit - x.TotalCredit
                );
            }
        }

        private class AccountBalanceQueryModel
        {
            public int ChartOfAccountId { get; set; }
            public double TotalDebit { get; set; }
            public double TotalCredit { get; set; }
        }
    }
}
