using Dapper;
using Domain.Entities.DTOs;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Repositories
{
    public class PaymentMethodRepository : GenericRepository<PaymentMethod, NameBaseEntityFilter>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<DataResultDTO<PaymentMethod>> GetAllIncludingInactive(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var data = await _db.QueryAsync<PaymentMethod>($"SELECT * FROM {_tableName} ORDER BY Id");
                var result = new DataResultDTO<PaymentMethod>
                {
                    Data = data,
                    TotalData = data.Count()
                };

                return result;
            }
        }
    }
}
