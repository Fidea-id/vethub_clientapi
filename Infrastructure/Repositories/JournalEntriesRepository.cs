using Domain.Entities.Models.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class JournalEntriesRepository : GenericRepository<JournalEntries, JournalEntriesFilter>, IJournalEntriesRepository
    {
        public JournalEntriesRepository(IDBFactory dbFactory) : base(dbFactory)
        {
        }
    }
}
