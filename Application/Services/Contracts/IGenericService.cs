using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Contracts
{
    public interface IGenericService<T, TRequest, TResponse, TFilter>
          where T : BaseEntity
          where TRequest : class
          where TResponse : class
          where TFilter : BaseEntityFilter
    {
        //Create
        Task<TResponse> CreateAsync(T entity, string dbName);
        //Read
        Task<IEnumerable<TResponse>> ReadAllAsync(string dbName);
        Task<IEnumerable<T>> GetEntitiesByFilter(TFilter filters, string dbName);
        Task<IEnumerable<TResponse>> ReadAllActiveAsync(string dbName);
        Task<T> ReadByIdAsync(int id, string dbName);
        Task<TResponse> ReadByIdResponseAsync(int id, string dbName);
        //Update
        Task<T> UpdateAsync(int id, TRequest entity, string dbName);
        Task DeactiveAsync(int id, string dbName);
        //Delete
        Task DeleteAsync(int id, string dbName);
        Task<string> UploadFileAsync(IFormFile file, string dbName);
        int GetEntityId(T entity);
    }
}
