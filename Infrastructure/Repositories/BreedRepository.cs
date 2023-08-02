using Dapper;
using DevExpress.Utils.About;
using Domain.Entities.DTOs;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Repositories
{
    public class BreedRepository : GenericRepository<Breeds, NameBaseEntityFilter>, IBreedRepository
    {
        public BreedRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<BreedAnimalResponse> GetBreedAnimal(int id, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            const string query = @"
            SELECT b.*, a.Name AS AnimalName
            FROM Breeds b
            JOIN Animals a ON a.Id = b.AnimalsId
            WHERE b.Id = @Id";
            var result = await _db.QueryFirstAsync<BreedAnimalResponse>(query, new { Id = id });
            return result;
        }

        public async Task<DataResultDTO<BreedAnimalResponse>> GetBreedAnimalList(NameBaseEntityFilter filter, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var mainTableName = "Breeds";
            var joinQuery = "JOIN Animals ON Animals.Id = Breeds.AnimalsId"; 
            var selectColumns = new List<string> { "Breeds.*", "Animals.Name as AnimalName" };
            var filterQuery = QueryGenerator.GenerateFilterQuery(filter, mainTableName, joinQuery, selectColumns);
            var queryString = filterQuery.Item1;
            var countQuery = QueryGenerator.GenerateSelectOrCountQuery(filterQuery.Item1, true);
            var countData = await _db.QueryFirstOrDefaultAsync<int>(countQuery, filterQuery.Item2);
            if (filter.Take.HasValue || filter.Skip.HasValue)
            {
                queryString = QueryGenerator.GenerateFilteredLimitQuery(queryString, filter.Skip, filter.Take);
            }
            var data = await _db.QueryAsync<BreedAnimalResponse>(queryString, filterQuery.Item2);
            var result = new DataResultDTO<BreedAnimalResponse>
            {
                Data = data,
                TotalData = countData
            };
            return result;
        }

        public async Task<IEnumerable<BreedAnimalResponse>> GetBreedAnimalListByAnimal(int idAnimal, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            const string query = @"
            SELECT b.*, a.Name AS AnimalName
            FROM Breeds b
            JOIN Animals a ON a.Id = b.AnimalsId
            WHERE a.Id = @Id";
            var result = await _db.QueryAsync<BreedAnimalResponse>(query, new { Id = idAnimal });
            return result;
        }
    }
}
