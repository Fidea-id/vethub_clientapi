﻿using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;

namespace Application.Services.Contracts
{
    public interface IAdditionalDataService
    {
        //animal
        Task<Animals> CreateAnimalAsync(AnimalsRequest entity, string dbName); 
        Task<IEnumerable<Animals>> ReadAnimalAllAsync(NameBaseEntityFilter filter, string dbName);
        Task<Animals> ReadAnimalByIdAsync(int id, string dbName); 
        Task<Animals> UpdateAnimalAsync(int id, AnimalsRequest entity, string dbName);
        Task DeleteAnimalAsync(int id, string dbName);
        //breed
        Task<Breeds> CreateBreedAsync(BreedsRequest entity, string dbName);
        Task<IEnumerable<Breeds>> ReadBreedAllAsync(NameBaseEntityFilter filter, string dbName);
        Task<Breeds> ReadBreedByIdAsync(int id, string dbName);
        Task<Breeds> ReadBreedByIdAnimalAsync(int idAnimal, string dbName);
        Task<Breeds> UpdateBreedAsync(int id, BreedsRequest entity, string dbName);
        Task DeleteBreedAsync(int id, string dbName);
    }
}