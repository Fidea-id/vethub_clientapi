using Domain.Entities.DTOs;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IAdditionalDataService
    {
        //animal
        Task<Animals> CreateAnimalAsync(AnimalsRequest entity, string dbName);
        Task<DataResultDTO<Animals>> ReadAnimalAllAsync(NameBaseEntityFilter filter, string dbName);
        Task<Animals> ReadAnimalByIdAsync(int id, string dbName);
        Task<Animals> UpdateAnimalAsync(int id, AnimalsRequest entity, string dbName);
        Task DeleteAnimalAsync(int id, string dbName);

        //breed
        Task<Breeds> CreateBreedAsync(BreedsRequest entity, string dbName);
        Task<DataResultDTO<BreedAnimalResponse>> ReadBreedAllAsync(NameBaseEntityFilter filter, string dbName);
        Task<BreedAnimalResponse> ReadBreedByIdAsync(int id, string dbName);
        Task<IEnumerable<BreedAnimalResponse>> ReadBreedByIdAnimalAsync(int idAnimal, string dbName);
        Task<Breeds> UpdateBreedAsync(int id, BreedsRequest entity, string dbName);
        Task DeleteBreedAsync(int id, string dbName);

        //Diagnoses
        Task<Diagnoses> CreateDiagnoseAsync(DiagnosesRequest entity, string dbName);
        Task<DataResultDTO<Diagnoses>> ReadDiagnoseAllAsync(NameBaseEntityFilter filter, string dbName);
        Task<Diagnoses> ReadDiagnoseByIdAsync(int id, string dbName);
        Task<Diagnoses> UpdateDiagnoseAsync(int id, DiagnosesRequest entity, string dbName);
        Task DeleteDiagnoseAsync(int id, string dbName);
    }
}
