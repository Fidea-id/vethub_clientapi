using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IGenericRepository<T, TFilter> where T : class where TFilter : BaseEntityFilter

    {
        Task<T> GetById(string dbName, int id);
        Task<IEnumerable<T>> GetAll(string dbName);
        Task<IEnumerable<T>> GetByFilter(string dbName, TFilter filters);
        Task<IEnumerable<T>> GetAllActive(string dbName);
        Task<int> Add(string dbName, T entity);
        Task AddRange(string dbName, IEnumerable<T> entity);
        Task Update(string dbName, T entity);
        Task Remove(string dbName, int id);
        Task UpdateRange(string dbName, IEnumerable<T> entity);
        Task RemoveRange(string dbName, IEnumerable<T> entity);
        Task<int> CountWithFilter(string dbName, TFilter filter);
    }
}
