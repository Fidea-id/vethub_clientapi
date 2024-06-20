using Dapper;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OpnamePatientsRepository : GenericRepository<OpnamePatients, OpnamePatientsFilter>, IOpnamePatientsRepository
    {
        public OpnamePatientsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<DataResultDTO<OpnamePatients>> GetByMedId(string dbName, int id)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var data = await _db.QueryAsync<OpnamePatients>($"SELECT * FROM OpnamePatients WHERE MedicalRecordId = @Id", new { Id = id });
            var result = new DataResultDTO<OpnamePatients>
            {
                Data = data,
                TotalData = data.Count()
            };
            return result;
        }

        public async Task<DataResultDTO<OpnamePatients>> GetByOpnameId(string dbName, int id)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var data = await _db.QueryAsync<OpnamePatients>($"SELECT * FROM OpnamePatients WHERE OpnameId = @Id", new { Id = id });
            var result = new DataResultDTO<OpnamePatients>
            {
                Data = data,
                TotalData = data.Count()
            };
            return result;
        }
    }
}
