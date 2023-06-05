using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGenericRepository<T>
    {
        Task<T> GetById(string dbName, int id);
        Task<IEnumerable<T>> GetAll(string dbName);
        Task<IEnumerable<T>> GetByFilter(string dbName, Dictionary<string, object> filters);
        Task<IEnumerable<T>> GetAllActive(string dbName);
        Task Add(string dbName, T entity);
        Task Update(string dbName, T entity);
        Task Delete(string dbName, int id);
    }
}
