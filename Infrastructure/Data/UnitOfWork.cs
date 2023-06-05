using Domain.Entities;
using Domain.Entities.Models;
using Domain.Interfaces;
using Infrastructure.Repositories;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDBFactory _dbFactory;
        private readonly Dictionary<string, IDbConnection> _connectionCache;

        public UnitOfWork(IDBFactory dBFactory)
        {
            _dbFactory = dBFactory;
            _connectionCache = _dbFactory.GetConnectionCache();
        }

        public void Dispose()
        {
            // Dispose all the connections in the connection cache
            foreach (var connection in _connectionCache.Values)
            {
                connection.Dispose();
            }
            _connectionCache.Clear();
        }

        // standard repository variable name
        // example :
        // private readonly TaskRepository _taskRepository
        // public ITaskRepository TaskRepository { get .....

        // add your repository here.
        // here just an example.

        private IProfileRepository _ProfileRepository;
        public IProfileRepository ProfileRepository
        {
            get
            {
                if (_ProfileRepository == null)
                {
                    _ProfileRepository = new ProfileRepository(_dbFactory);
                }
                return _ProfileRepository;
            }
        }
        private IServicesRepository _ServicesRepository;
        public IServicesRepository ServicesRepository
        {
            get
            {
                if (_ServicesRepository == null)
                {
                    _ServicesRepository = new ServicesRepository(_dbFactory);
                }
                return _ServicesRepository;
            }
        }
    }
}
