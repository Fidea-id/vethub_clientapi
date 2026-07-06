using Dapper;
using Domain.Entities.DTOs;
using Domain.Entities.Models.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class FinancialTransactionsRepository : GenericRepository<FinancialTransactions, FinancialTransactionsFilter>, IFinancialTransactionsRepository
    {
        public FinancialTransactionsRepository(IDBFactory dbFactory) : base(dbFactory)
        {
        }

        public override async Task<DataResultDTO<FinancialTransactions>> GetByFilter(string dbName, FinancialTransactionsFilter filters)
        {
            using (var db = _dbFactory.GetDbConnection(dbName))
            {
                var conditions = new List<string>();
                var parameters = new DynamicParameters();

                // Base IsActive filter
                conditions.Add("IsActive = 1");

                // StartDate filter (TransactionDate >= StartDate)
                if (filters.StartDate.HasValue)
                {
                    conditions.Add("TransactionDate >= @StartDate");
                    parameters.Add("@StartDate", filters.StartDate.Value);
                }

                // EndDate filter (TransactionDate <= EndDate)
                if (filters.EndDate.HasValue)
                {
                    var endDate = filters.EndDate.Value;
                    if (endDate.TimeOfDay == TimeSpan.Zero)
                    {
                        endDate = endDate.Date.AddDays(1).AddTicks(-1);
                    }
                    conditions.Add("TransactionDate <= @EndDate");
                    parameters.Add("@EndDate", endDate);
                }

                // ReferenceNo filter
                if (!string.IsNullOrEmpty(filters.ReferenceNo))
                {
                    conditions.Add("ReferenceNo LIKE @ReferenceNo");
                    parameters.Add("@ReferenceNo", $"%{filters.ReferenceNo}%");
                }

                // TransactionType filter
                if (!string.IsNullOrEmpty(filters.TransactionType))
                {
                    conditions.Add("TransactionType = @TransactionType");
                    parameters.Add("@TransactionType", filters.TransactionType);
                }

                // Generic Search filter
                if (!string.IsNullOrEmpty(filters.Search))
                {
                    conditions.Add("(ReferenceNo LIKE @Search OR Description LIKE @Search OR VoucherNo LIKE @Search)");
                    parameters.Add("@Search", $"%{filters.Search}%");
                }

                var whereClause = string.Join(" AND ", conditions);
                var baseQuery = $"FROM {_tableName} WHERE {whereClause}";

                var countQuery = $"SELECT COUNT(*) {baseQuery}";
                var countData = await db.QueryFirstOrDefaultAsync<int>(countQuery, parameters);

                // Handle sorting
                var sortClause = "ORDER BY TransactionDate DESC";
                if (!string.IsNullOrEmpty(filters.SortProp))
                {
                    var sortMode = string.IsNullOrEmpty(filters.SortMode) ? "ASC" : filters.SortMode.ToUpper();
                    sortClause = $"ORDER BY {filters.SortProp} {sortMode}";
                }

                var queryString = $"SELECT * {baseQuery} {sortClause}";

                // Handle pagination
                if (filters.Take.HasValue || filters.Skip.HasValue)
                {
                    var skip = filters.Skip ?? 0;
                    var take = filters.Take ?? 10;
                    queryString += $" LIMIT {skip}, {take}";
                }

                var data = await db.QueryAsync<FinancialTransactions>(queryString, parameters);

                return new DataResultDTO<FinancialTransactions>
                {
                    Data = data,
                    TotalData = countData
                };
            }
        }
    }
}
