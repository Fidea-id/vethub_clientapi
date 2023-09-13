using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities;
using Domain.Entities.DTOs;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.Implementations
{
    public class GenericService<T, TRequest, TResponse, TFilter> : IGenericService<T, TRequest, TResponse, TFilter>
            where T : BaseEntity
            where TRequest : class
            where TResponse : class
            where TFilter : BaseEntityFilter
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IGenericRepository<T, TFilter> _repository;

        public GenericService(IUnitOfWork unitOfWork, IGenericRepository<T, TFilter> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<TResponse> CreateAsync(T entity, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(entity);
                FormatUtil.SetIsActive<T>(entity, true);
                FormatUtil.SetDateBaseEntity<T>(entity);

                var newId = await _repository.Add(dbName, entity);
                entity.Id = newId;
                return Mapping.Mapper.Map<TResponse>(entity);
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.CreateAsync";
                throw;
            }
        }

        public async Task DeleteAsync(int id, string? dbName)
        {
            try
            {
                //get entity
                var entity = await _repository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _repository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.DeleteAsync";
                throw;
            }
        }

        public async Task<T> UpdateAsync(int id, TRequest request, string? dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<T>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<T>(entity, true);

                T checkedEntity = await _repository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<T, T>(entity, checkedEntity);
                FormatUtil.SetIsActive<T>(checkedEntity, true);
                await _repository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.UpdateAsync";
                throw;
            }
        }

        public async Task<IEnumerable<TResponse>> ReadAllAsync(string dbName)
        {
            try
            {
                var data = await _repository.GetAll(dbName);
                return Mapping.Mapper.Map<IEnumerable<TResponse>>(data);
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.ReadAllAsync";
                throw;
            }
        }

        public async Task<IEnumerable<TResponse>> ReadAllActiveAsync(string dbName)
        {
            try
            {
                var data = await _repository.GetAllActive(dbName);
                return Mapping.Mapper.Map<IEnumerable<TResponse>>(data);
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.ReadAllActiveAsync";
                throw;
            }
        }

        public async Task<DataResultDTO<T>> GetEntitiesByFilter(TFilter filters, string dbName)
        {
            return await _repository.GetByFilter(dbName, filters);
        }

        public async Task<T> ReadByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _repository.GetById(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.ReadByIdAsync";
                throw;
            }
        }

        public async Task<TResponse> ReadByIdResponseAsync(int id, string dbName)
        {
            try
            {
                var data = await _repository.GetById(dbName, id);
                return Mapping.Mapper.Map<TResponse>(data);
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.ReadByIdResponseAsync";
                throw;
            }
        }

        public int GetEntityId(T entity)
        {
            try
            {
                var type = entity.GetType();
                var keyProperty = type.GetProperties().First(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any());
                return (int)keyProperty.GetValue(entity);
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.GetEntityId";
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string? dbName)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new Exception("No file provided");
                }
                var folder = typeof(T).Name;

                var baseUrl = "";
                var uniqueFileName = FormatUtil.GenerateUniqueFileName(file.FileName);
                var url = $"{baseUrl}{folder}/{uniqueFileName}";

                // Get the full path to the wwwroot directory
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", folder);
                string filePath = Path.Combine(folderPath, uniqueFileName);


                // Check if the target folder exists
                if (!Directory.Exists(folderPath))
                {
                    // Create the folder if it does not exist
                    Directory.CreateDirectory(folderPath);
                }

                // Check if the target file already exists
                while (File.Exists(filePath))
                {
                    // Generate a new unique file name if the file already exists
                    uniqueFileName = FormatUtil.GenerateUniqueFileName(file.FileName);
                    filePath = Path.Combine(folderPath, uniqueFileName);
                }

                // Validate file type
                if (!IsValidImageType(file.ContentType))
                {
                    throw new Exception("Invalid file type. Only JPEG and PNG are allowed.");
                }

                // Open the file stream
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    // Copy the file data to the wwwroot directory
                    await file.OpenReadStream().CopyToAsync(fileStream);
                }
                return url;
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.UploadFileAsync";
                throw;
            }
        }
        private bool IsValidImageType(string contentType)
        {
            // Allow only JPEG and PNG
            return new[] { "image/jpeg", "image/png" }.Contains(contentType);
        }

        public async Task DeactiveAsync(int id, string dbName)
        {
            try
            {
                T checkedEntity = await _repository.GetById(dbName, id);
                //convert entity
                FormatUtil.SetOppositeActive<T>(checkedEntity);
                FormatUtil.SetDateBaseEntity<T>(checkedEntity, true);
                await _repository.Update(dbName, checkedEntity);
            }
            catch (Exception ex)
            {
                ex.Source = typeof(T).Name;
                throw;
            }
        }

        public async Task<TResponse> CreateRequestAsync(TRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<T>(request);
                FormatUtil.SetIsActive<T>(entity, true);
                FormatUtil.SetDateBaseEntity<T>(entity);

                var newId = await _repository.Add(dbName, entity);
                entity.Id = newId;
                return Mapping.Mapper.Map<TResponse>(entity);
            }
            catch (Exception ex)
            {
                ex.Source = $"{typeof(T).Name}Service.CreateRequestAsync";
                throw;
            }
        }
    }
}
