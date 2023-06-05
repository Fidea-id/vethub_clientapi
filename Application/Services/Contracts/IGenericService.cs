using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Contracts
{
    public interface IGenericService<T, TResponse>
          where T : class
          where TResponse : class
    {
        //Create
        Task<TResponse> CreateAsync(T entity, string dbName);
        //Read
        Task<IEnumerable<TResponse>> ReadAllAsync(string dbName);
        Task<IEnumerable<T>> GetEntitiesByFilter(Dictionary<string, object> filters, string dbName);
        Task<IEnumerable<TResponse>> ReadAllActiveAsync(string dbName);
        Task<T> ReadByIdAsync(int id, string dbName);
        Task<TResponse> ReadByIdResponseAsync(int id, string dbName);
        //Update
        Task UpdateAsync(int id, T entity, string dbName);
        Task DeactiveAsync(int id, string dbName);
        //Delete
        Task DeleteAsync(int id, string dbName);
        Task<string> UploadFileAsync(IFormFile file, string dbName);
        int GetEntityId(T entity);
    }
}
