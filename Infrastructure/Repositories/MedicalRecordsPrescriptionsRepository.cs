using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class MedicalRecordsPrescriptionsRepository : GenericRepository<MedicalRecordsPrescriptions, MedicalRecordsPrescriptionsFilter>, IMedicalRecordsPrescriptionsRepository
    {
        public MedicalRecordsPrescriptionsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<MedicalRecordsPrescriptions>> GetByMedicalRecordId(string dbName, int medicalRecordsId)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                return await _db.QueryAsync<MedicalRecordsPrescriptions>($"SELECT * FROM MedicalRecordsPrescriptions WHERE MedicalRecordsId = @Id AND IsActive = 1", new { Id = medicalRecordsId });
            }
        }

        public async Task<IEnumerable<FrequentDiagnoseMeds>> GetMedsFrequency(string dbName, string date)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var resultQuery = $@"WITH RankedData AS (
                                SELECT md.Diagnose AS Name, 'Diagnose' AS Type, COUNT(md.Id) AS Total, ROW_NUMBER() OVER (PARTITION BY 'Diagnose' ORDER BY COUNT(md.Id) DESC) AS RowNum
                                FROM MedicalRecordsDiagnoses md
                                JOIN MedicalRecords mr ON md.MedicalRecordsId = mr.Id
                                WHERE md.IsActive = 1 AND md.{date}
                                GROUP BY md.Diagnose

                                UNION ALL

                                SELECT mrp.ProductName AS Name, 'Products' AS Type, COUNT(mrp.Id) AS Total, ROW_NUMBER() OVER (PARTITION BY 'Products' ORDER BY COUNT(mrp.Id) DESC) AS RowNum
                                FROM MedicalRecordsPrescriptions mrp
                                JOIN MedicalRecords mr ON mrp.MedicalRecordsId = mr.Id
                                WHERE mrp.IsActive = 1 AND mrp.Type = 'Product' AND mrp.{date}
                                GROUP BY mrp.ProductName
                            )
                            SELECT Name, Type, Total
                            FROM RankedData
                            WHERE RowNum <= 5
                            ORDER BY Type, Total DESC;";
                return await _db.QueryAsync<FrequentDiagnoseMeds>(resultQuery);
            }
        }
    }
}
