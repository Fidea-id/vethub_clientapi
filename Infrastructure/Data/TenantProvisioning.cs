using Dapper;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using Domain.Entities.Models.Clients.XPO;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data
{
    public interface ITenantProvisioning
    {
        Task ProvisionTenantAsync(string dbName);
    }
    public class TenantProvisioning : ITenantProvisioning
    {
        private readonly IDBFactory _dbFactory;
        private readonly ILogger<TenantProvisioning> _logger;

        public TenantProvisioning(IDBFactory dbFactory, ILogger<TenantProvisioning> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task ProvisionTenantAsync(string dbName)
        {
            // Step 1: Buat DB kalau belum ada
            using (var masterConnection = _dbFactory.GetMasterDbConnection())
            {
                var dbExists = await masterConnection.ExecuteScalarAsync<string>(
                    "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @dbName",
                    new { dbName }
                );

                if (dbExists == null)
                {
                    _logger.LogInformation("Creating database {dbName}", dbName);
                    await masterConnection.ExecuteAsync($"CREATE DATABASE `{dbName}`;");
                }
            }

            // Step 2: Jalankan XPO untuk create / update schema
            AutoCreateDB(dbName);
        }

        private void AutoCreateDB(string dbName)
        {
            string rawConn = _dbFactory.GetConnectionString(dbName);
            var entityTypes = new List<Type>
            {
                typeof(SchemaVersionXPO),
                typeof(ProfileXPO),
                typeof(ServicesXPO),
                typeof(AnimalsXPO),
                typeof(BreedsXPO),
                typeof(OwnersXPO),
                typeof(PatientsXPO),
                typeof(ProductsXPO),
                typeof(ProductStocksXPO),
                typeof(ProductBundlesXPO),
                typeof(ProductDiscountsXPO),
                typeof(ProductCategoriesXPO),
                typeof(AppointmentsXPO),
                typeof(AppointmentsActivityXPO),
                typeof(AppointmentsStatusXPO),
                typeof(PatientsStatisticXPO),
                typeof(DiagnosesXPO),
                typeof(OrdersXPO),
                typeof(OrdersDetailXPO),
                typeof(OrdersPaymentXPO),
                typeof(ClinicsXPO),
                typeof(PaymentMethodXPO),
                typeof(MedicalRecordsXPO),
                typeof(MedicalRecordsDiagnosesXPO),
                typeof(MedicalRecordsNotesXPO),
                typeof(MedicalRecordsPrescriptionsXPO),
                typeof(PrescriptionFrequentsXPO),
                typeof(NotificationsXPO),
                typeof(ProductStockHistoricalXPO),
                typeof(OpnamePatientsXPO),
                typeof(OpnamesXPO),
                typeof(EventLogsXPO),
                typeof(ClinicConfigXPO),
                typeof(AppointmentsTypeXPO),
                typeof(MixedMedicineCompositionXPO),
                typeof(MixedMedicineXPO),
                typeof(ChartOfAccountsXPO),
                // Add more model types here
            };

            string xpoConn = $"XpoProvider=MySql;{rawConn}";

            _logger.LogInformation("Using XPO connection: {conn}", xpoConn);

            // Tunggu 0.5–1 detik untuk memastikan DB siap setelah CREATE DATABASE
            Thread.Sleep(800);

            XPDictionary dict = new ReflectionDictionary();
            using var dataLayer = XpoDefault.GetDataLayer(xpoConn, dict, AutoCreateOption.DatabaseAndSchema);
            dataLayer.UpdateSchema(false, dict.CollectClassInfos(entityTypes));
        }
    }
}
