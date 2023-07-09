using Domain.Interfaces.Clients;
using Infrastructure.Repositories;
using System.Data;

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

        private IOwnersRepository _OwnersRepository;
        public IOwnersRepository OwnersRepository
        {
            get
            {
                if (_OwnersRepository == null)
                {
                    _OwnersRepository = new OwnersRepository(_dbFactory);
                }
                return _OwnersRepository;
            }
        }

        private IPatientsRepository _PatientsRepository;
        public IPatientsRepository PatientsRepository
        {
            get
            {
                if (_PatientsRepository == null)
                {
                    _PatientsRepository = new PatientsRepository(_dbFactory);
                }
                return _PatientsRepository;
            }
        }

        private IProductsRepository _ProductsRepository;
        public IProductsRepository ProductsRepository
        {
            get
            {
                if (_ProductsRepository == null)
                {
                    _ProductsRepository = new ProductsRepository(_dbFactory);
                }
                return _ProductsRepository;
            }
        }
        private IAppointmentRepository _AppointmentRepository;
        public IAppointmentRepository AppointmentRepository
        {
            get
            {
                if (_AppointmentRepository == null)
                {
                    _AppointmentRepository = new AppointmentRepository(_dbFactory);
                }
                return _AppointmentRepository;
            }
        }

        public IGenerateTableRepository _GenerateTableRepository;

        public IGenerateTableRepository GenerateTableRepository
        {
            get
            {
                if (_GenerateTableRepository == null)
                {
                    _GenerateTableRepository = new GenerateTableRepository(_dbFactory);
                }
                return _GenerateTableRepository;
            }
        }
    }
}
