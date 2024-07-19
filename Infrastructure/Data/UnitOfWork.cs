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
        private IProductBundlesRepository _ProductBundlesRepository;
        public IProductBundlesRepository ProductBundlesRepository
        {
            get
            {
                if (_ProductBundlesRepository == null)
                {
                    _ProductBundlesRepository = new ProductBundlesRepository(_dbFactory);
                }
                return _ProductBundlesRepository;
            }
        }
        private IProductCategoriesRepository _ProductCategoriesRepository;
        public IProductCategoriesRepository ProductCategoriesRepository
        {
            get
            {
                if (_ProductCategoriesRepository == null)
                {
                    _ProductCategoriesRepository = new ProductCategoriesRepository(_dbFactory);
                }
                return _ProductCategoriesRepository;
            }
        }

        private IProductStockRepository _ProductStockRepository;
        public IProductStockRepository ProductStockRepository
        {
            get
            {
                if (_ProductStockRepository == null)
                {
                    _ProductStockRepository = new ProductStockRepository(_dbFactory);
                }
                return _ProductStockRepository;
            }
        }
        private IProductDiscountsRepository _ProductDiscountsRepository;
        public IProductDiscountsRepository ProductDiscountsRepository
        {
            get
            {
                if (_ProductDiscountsRepository == null)
                {
                    _ProductDiscountsRepository = new ProductDiscountsRepository(_dbFactory);
                }
                return _ProductDiscountsRepository;
            }
        }
        private IAnimalRepository _AnimalRepository;
        public IAnimalRepository AnimalRepository
        {
            get
            {
                if (_AnimalRepository == null)
                {
                    _AnimalRepository = new AnimalRepository(_dbFactory);
                }
                return _AnimalRepository;
            }
        }
        private IBreedRepository _BreedRepository;
        public IBreedRepository BreedRepository
        {
            get
            {
                if (_BreedRepository == null)
                {
                    _BreedRepository = new BreedRepository(_dbFactory);
                }
                return _BreedRepository;
            }
        }
        private IDiagnosesRepository _DiagnoseRepository;
        public IDiagnosesRepository DiagnoseRepository
        {
            get
            {
                if (_DiagnoseRepository == null)
                {
                    _DiagnoseRepository = new DiagnosesRepository(_dbFactory);
                }
                return _DiagnoseRepository;
            }
        }
        private IPatientsStatisticRepository _PatientsStatisticRepository;
        public IPatientsStatisticRepository PatientsStatisticRepository
        {
            get
            {
                if (_PatientsStatisticRepository == null)
                {
                    _PatientsStatisticRepository = new PatientsStatisticRepository(_dbFactory);
                }
                return _PatientsStatisticRepository;
            }
        }
        private IOrdersRepository _OrdersRepository;
        public IOrdersRepository OrdersRepository
        {
            get
            {
                if (_OrdersRepository == null)
                {
                    _OrdersRepository = new OrdersRepository(_dbFactory);
                }
                return _OrdersRepository;
            }
        }
        private IOrdersDetailRepository _OrdersDetailRepository;
        public IOrdersDetailRepository OrdersDetailRepository
        {
            get
            {
                if (_OrdersDetailRepository == null)
                {
                    _OrdersDetailRepository = new OrdersDetailRepository(_dbFactory);
                }
                return _OrdersDetailRepository;
            }
        }
        private IOrdersPaymentRepository _OrdersPaymentRepository;
        public IOrdersPaymentRepository OrdersPaymentRepository
        {
            get
            {
                if (_OrdersPaymentRepository == null)
                {
                    _OrdersPaymentRepository = new OrdersPaymentRepository(_dbFactory);
                }
                return _OrdersPaymentRepository;
            }
        }
        private IPaymentMethodRepository _PaymentMethodRepository;
        public IPaymentMethodRepository PaymentMethodRepository
        {
            get
            {
                if (_PaymentMethodRepository == null)
                {
                    _PaymentMethodRepository = new PaymentMethodRepository(_dbFactory);
                }
                return _PaymentMethodRepository;
            }
        }
        private IClinicRepository _ClinicRepository;
        public IClinicRepository ClinicsRepository
        {
            get
            {
                if (_ClinicRepository == null)
                {
                    _ClinicRepository = new ClinicRepository(_dbFactory);
                }
                return _ClinicRepository;
            }
        }

        private IMedicalRecordsRepository _MedicalRecordsRepository;
        public IMedicalRecordsRepository MedicalRecordsRepository
        {
            get
            {
                if (_MedicalRecordsRepository == null)
                {
                    _MedicalRecordsRepository = new MedicalRecordsRepository(_dbFactory);
                }
                return _MedicalRecordsRepository;
            }
        }
        private IMedicalRecordsPrescriptionsRepository _MedicalRecordsPrescriptionsRepository;
        public IMedicalRecordsPrescriptionsRepository MedicalRecordsPrescriptionsRepository
        {
            get
            {
                if (_MedicalRecordsPrescriptionsRepository == null)
                {
                    _MedicalRecordsPrescriptionsRepository = new MedicalRecordsPrescriptionsRepository(_dbFactory);
                }
                return _MedicalRecordsPrescriptionsRepository;
            }
        }
        private IMedicalRecordsNotesRepository _MedicalRecordsNotesRepository;
        public IMedicalRecordsNotesRepository MedicalRecordsNotesRepository
        {
            get
            {
                if (_MedicalRecordsNotesRepository == null)
                {
                    _MedicalRecordsNotesRepository = new MedicalRecordsNotesRepository(_dbFactory);
                }
                return _MedicalRecordsNotesRepository;
            }
        }
        private IMedicalRecordsDiagnosesRepository _MedicalRecordsDiagnosesRepository;
        public IMedicalRecordsDiagnosesRepository MedicalRecordsDiagnosesRepository
        {
            get
            {
                if (_MedicalRecordsDiagnosesRepository == null)
                {
                    _MedicalRecordsDiagnosesRepository = new MedicalRecordsDiagnosesRepository(_dbFactory);
                }
                return _MedicalRecordsDiagnosesRepository;
            }
        }
        private IPrescriptionFrequentsRepository _PrescriptionFrequentsRepository;
        public IPrescriptionFrequentsRepository PrescriptionFrequentsRepository
        {
            get
            {
                if (_PrescriptionFrequentsRepository == null)
                {
                    _PrescriptionFrequentsRepository = new PrescriptionFrequentsRepository(_dbFactory);
                }
                return _PrescriptionFrequentsRepository;
            }
        }
        private INotificationsRepository _NotificationsRepository;
        public INotificationsRepository NotificationsRepository
        {
            get
            {
                if (_NotificationsRepository == null)
                {
                    _NotificationsRepository = new NotificationsRepository(_dbFactory);
                }
                return _NotificationsRepository;
            }
        }
        private IProductStockHistoricalRepository _ProductStockHistoricalRepository;
        public IProductStockHistoricalRepository ProductStockHistoricalRepository
        {
            get
            {
                if (_ProductStockHistoricalRepository == null)
                {
                    _ProductStockHistoricalRepository = new ProductStockHistoricalRepository(_dbFactory);
                }
                return _ProductStockHistoricalRepository;
            }
        }
        private IOpnamePatientsRepository _OpnamePatientsRepository;
        public IOpnamePatientsRepository OpnamePatientsRepository
        {
            get
            {
                if (_OpnamePatientsRepository == null)
                {
                    _OpnamePatientsRepository = new OpnamePatientsRepository(_dbFactory);
                }
                return _OpnamePatientsRepository;
            }
        }
        private IOpnamesRepository _OpnamesRepository;
        public IOpnamesRepository OpnamesRepository
        {
            get
            {
                if (_OpnamesRepository == null)
                {
                    _OpnamesRepository = new OpnamesRepository(_dbFactory);
                }
                return _OpnamesRepository;
            }
        }
        private IEventLogRepository _EventLogRepository;
        public IEventLogRepository EventLogRepository
        {
            get
            {
                if (_EventLogRepository == null)
                {
                    _EventLogRepository = new EventLogRepository(_dbFactory);
                }
                return _EventLogRepository;
            }
        }

        private IClinicConfigRepository _ClinicConfigRepository;
        public IClinicConfigRepository ClinicConfigRepository
        {
            get
            {
                if (_ClinicConfigRepository == null)
                {
                    _ClinicConfigRepository = new ClinicConfigRepository(_dbFactory);
                }
                return _ClinicConfigRepository;
            }
        }
    }
}
