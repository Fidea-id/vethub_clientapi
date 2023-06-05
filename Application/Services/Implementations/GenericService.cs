using Application.Services.Contracts;
using Application.Utils;
using Domain.Interfaces;
using Domain.Utils;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.Implementations
{
    public class GenericService<T, TResponse> : IGenericService<T, TResponse>
            where T : class
            where TResponse : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IGenericRepository<T> _repository;

        public GenericService(IUnitOfWork unitOfWork, IGenericRepository<T> repository)
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

                //begin transaction
                //await _unitOfWork.BeginTransaction();
                //create entity
                await _repository.Add(dbName, entity);

                //string output;
                //FormatUtil.GetPropValue(entity, typeof(T).Name, out output);

                //create log
                //var log = LoggingUtil.CreateLogChangeTable(typeof(T).Name, dbName, "Insert", 0);
                //await _unitOfWork.LogChangeRepository.Add(log);

                //complete transaction
                //await _unitOfWork.CommitTransaction();

                return Mapping.Mapper.Map<TResponse>(entity);
            }
            catch (Exception ex)
            {
                //rollback transaction
                //await _unitOfWork.RollbackTransaction();
                ex.Source = typeof(T).Name;
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

                //begin transaction
                //await _unitOfWork.BeginTransaction();

                //remove entity
                await _repository.Delete(dbName, id);

                //create log
                //var log = LoggingUtil.CreateLogChangeTable(typeof(T).Name, dbName, "Remove", id);
                //await _unitOfWork.LogChangeRepository.Add(log);

                //complete scope
                //await _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                //rollback transaction
                //await _unitOfWork.RollbackTransaction();
                ex.Source = typeof(T).Name;
                throw;
            }
        }

        public async Task UpdateAsync(int id, T entity, string? dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(entity);

                T checkedEntity = await _repository.GetById(dbName, id);
                //convert entity
                FormatUtil.ConvertUpdateObject<T, T>(entity, checkedEntity);
                FormatUtil.SetIsActive<T>(checkedEntity, true);
                //FormatUtil.SetLastUpdatedBy<T>(checkedEntity, dbName);
                //begin transaction
                //await _unitOfWork.BeginTransaction();
                //update entity
                await _repository.Update(dbName, checkedEntity);

                //var hasImageChanges = FormatUtil.HasImageProperty(entity);
                //if (hasImageChanges != null)
                //{
                //    //get version number
                //    var versionNumber = GetVersionActiveId();
                //    foreach (var imageChange in hasImageChanges)
                //    {
                //        //do create log image change
                //        var logImage = LoggingUtil.CreateLogImageTable(typeof(T).Name, dbName, imageChange, id, versionNumber);
                //        await _unitOfWork.LogUpdateImageRepository.Add(logImage);
                //    }
                //}

                //create log
                //var log = LoggingUtil.CreateLogChangeTable(typeof(T).Name, dbName, "Update", id);
                //await _unitOfWork.LogChangeRepository.Add(log);

                //complete scope
                //await _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                //rollback transaction
                //await _unitOfWork.RollbackTransaction();
                ex.Source = typeof(T).Name;
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
                ex.Source = typeof(T).Name;
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
                ex.Source = typeof(T).Name;
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetEntitiesByFilter(Dictionary<string, object> filters, string dbName)
        {
            // You can add additional business logic or validation here
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
                ex.Source = typeof(T).Name;
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
                ex.Source = typeof(T).Name;
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
                ex.Source = typeof(T).Name;
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

                //CompressImageThenSave(file, filePath, file.ContentType);
                // Open the file stream
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    // Copy the file data to the wwwroot directory
                    await file.OpenReadStream().CopyToAsync(fileStream);
                }
                //var log = LoggingUtil.CreateLogChangeUploadUser(dbName, uniqueFileName, typeof(T).Name);
                //begin transaction
                //await _unitOfWork.BeginTransaction();
                //create log
                //await _unitOfWork.LogChangeRepository.Add(log);
                //complete scope
                //await _unitOfWork.CommitTransaction();
                return url;
            }
            catch (Exception ex)
            {
                ex.Source = typeof(T).Name;
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
                //FormatUtil.SetLastUpdatedBy<T>(checkedEntity, dbName);

                //begin transaction
                //await _unitOfWork.BeginTransaction();
                //update entity
                await _repository.Update(dbName, checkedEntity);

                //create log
                //var log = LoggingUtil.CreateLogChangeTable(typeof(T).Name, dbName, "Active/Deactive", id);
                //await _unitOfWork.LogChangeRepository.Add(log);

                //complete scope
                //await _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                //rollback transaction
                //await _unitOfWork.RollbackTransaction();
                ex.Source = typeof(T).Name;
                throw;
            }
        }
    }
}
