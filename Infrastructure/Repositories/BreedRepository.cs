using Dapper;
using DevExpress.Utils.About;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;

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

        public async Task<IEnumerable<BreedAnimalResponse>> GetBreedAnimalList(NameBaseEntityFilter filter, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var mainTableName = "Breeds";
            var joinQuery = "JOIN Animals ON Animals.Id = Breeds.AnimalsId"; 
            var selectColumns = new List<string> { "Breeds.*", "Animals.Name as AnimalName" };
            var result = QueryGenerator.GenerateFilterQuery(filter, mainTableName, joinQuery, selectColumns);
            var ss = result.Item2.ToString();
            return await _db.QueryAsync<BreedAnimalResponse>(result.Item1, result.Item2);
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
