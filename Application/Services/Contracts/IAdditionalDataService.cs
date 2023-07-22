using Domain.Entities.Models.Clients;

namespace Application.Services.Contracts
{
    public interface IAdditionalDataService
    {
        //animal
        Task<Animals> CreateAnimalAsync(Animals entity, string dbName); 
        Task<IEnumerable<Animals>> ReadAnimalAllAsync(string dbName);
        Task<Animals> ReadAnimalByIdAsync(int id, string dbName); 
        Task<Animals> UpdateAnimalAsync(int id, Animals entity, string dbName);
        Task DeleteAnimalAsync(int id, string dbName);
        //breed
        Task<Breeds> CreateBreedAsync(Breeds entity, string dbName);
        Task<IEnumerable<Breeds>> ReadBreedAllAsync(string dbName);
        Task<Breeds> ReadBreedByIdAsync(int id, string dbName);
        Task<Breeds> ReadBreedByIdAnimalAsync(int idAnimal, string dbName);
        Task<Breeds> UpdateBreedAsync(int id, Breeds entity, string dbName);
        Task DeleteBreedAsync(int id, string dbName);
    }
}
