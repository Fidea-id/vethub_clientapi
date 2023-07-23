﻿using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;

namespace Application.Services.Implementations
{
    public class AdditionalDataService : IAdditionalDataService
    {
        private readonly IUnitOfWork _uow;

        public AdditionalDataService(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }

        #region Animals
        public async Task<Animals> CreateAnimalAsync(AnimalsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Animals>(request);
                FormatUtil.SetIsActive<Animals>(entity, true);
                FormatUtil.SetDateBaseEntity<Animals>(entity);

                var newId = await _uow.AnimalRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.CreateAnimalAsync";
                throw;
            }
        }
        public async Task<IEnumerable<Animals>> ReadAnimalAllAsync(NameBaseEntityFilter filter, string dbName)
        {
            try
            {
                var data = await _uow.AnimalRepository.GetByFilter(dbName, filter);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.ReadAllAnimalAsync";
                throw;
            }
        }
        public async Task<Animals> ReadAnimalByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _uow.AnimalRepository.GetById(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.ReadByIdAnimalAsync";
                throw;
            }
        }
        public async Task<Animals> UpdateAnimalAsync(int id, AnimalsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Animals>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<Animals>(entity, true);

                Animals checkedEntity = await _uow.AnimalRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<Animals, Animals>(entity, checkedEntity);
                FormatUtil.SetIsActive<Animals>(checkedEntity, true);
                await _uow.AnimalRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.UpdateAnimalAsync";
                throw;
            }
        }
        public async Task DeleteAnimalAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _uow.AnimalRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _uow.AnimalRepository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.DeleteAnimalAsync";
                throw;
            }
        }
        #endregion

        #region Breed

        public async Task<Breeds> CreateBreedAsync(BreedsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Breeds>(request);
                FormatUtil.SetIsActive<Breeds>(entity, true);
                FormatUtil.SetDateBaseEntity<Breeds>(entity);

                var newId = await _uow.BreedRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.CreateBreedAsync";
                throw;
            }
        }
        public async Task<IEnumerable<Breeds>> ReadBreedAllAsync(NameBaseEntityFilter filter, string dbName)
        {
            try
            {
                var data = await _uow.BreedRepository.GetByFilter(dbName, filter);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.ReadAllAnimalAsync";
                throw;
            }
        }
        public Task<Breeds> ReadBreedByIdAnimalAsync(int idAnimal, string dbName)
        {
            throw new NotImplementedException();
        }
        public async Task<Breeds> ReadBreedByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _uow.BreedRepository.GetById(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.ReadByIdAnimalAsync";
                throw;
            }
        }
        public async Task<Breeds> UpdateBreedAsync(int id, BreedsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Breeds>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<Breeds>(entity, true);

                Breeds checkedEntity = await _uow.BreedRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<Breeds, Breeds>(entity, checkedEntity);
                FormatUtil.SetIsActive<Breeds>(checkedEntity, true);
                await _uow.BreedRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.UpdateBreedAsync";
                throw;
            }
        }
        public async Task DeleteBreedAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _uow.BreedRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _uow.BreedRepository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"Service.DeleteBreedAsync";
                throw;
            }
        }
        #endregion
    }
}
