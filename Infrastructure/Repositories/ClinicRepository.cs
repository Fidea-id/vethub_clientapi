using Dapper;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ClinicRepository : GenericRepository<Clinics, ClinicsFilter>, IClinicRepository
    {
        public ClinicRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<ClinicServiceUsageDTO>> GetClinicServiceUsageReportsAsync(string db)
        {
            using (var _db = _dbFactory.GetDbConnection(db))
            {
                const string query = @"
               SELECT
                  s.Id                           AS ServiceId,
                  s.Name                         AS ServiceName,
                  CONCAT(COALESCE(s.Duration,0), ' ', COALESCE(s.DurationType,'')) AS DurationText,
                  ROUND(AVG(COALESCE(mrp.Price, s.Price, 0))) AS Price,
                  SUM(COALESCE(mrp.Quantity, 1)) AS TotalUsage
                FROM MedicalRecordsPrescriptions mrp
                JOIN MedicalRecords mr ON mr.Id = mrp.MedicalRecordsId
                JOIN Appointments a ON a.Id = mr.AppointmentId
                JOIN Services s ON s.Id = mrp.ProductId
                WHERE
                  mrp.IsActive = b'1'
                  AND mr.IsActive = b'1'
                  AND a.IsActive = b'1'
                  AND a.StatusId = 6
                  AND mrp.Type = 'Service'
                  AND s.IsActive = b'1'
                GROUP BY
                  s.Id, s.Name, s.Duration, s.DurationType
                ORDER BY TotalUsage DESC;

            ";

                var result = await _db.QueryAsync<ClinicServiceUsageDTO>(query);
                return result;
            }
        }

        public async Task<ClinicReportsClientDTO> GetClinicReportsAsync(string db)
        {
            using (var _db = _dbFactory.GetDbConnection(db))
            {
                const string query = @"
               SELECT 
                    c.Name AS ClinicName,
                    c.Id AS Entity, -- kalau Entity diset di client db, bisa diganti sesuai field
                    (SELECT COUNT(*) FROM Patients p WHERE p.IsActive = TRUE) AS TotalPatients,
                    (SELECT COUNT(*) FROM Appointments a WHERE a.IsActive = TRUE) AS TotalAppointments,
                    (SELECT COUNT(*) FROM MedicalRecords m WHERE m.IsActive = TRUE) AS TotalMedicalRecords,
                    (
                        COALESCE((SELECT SUM(m.TotalDiscounted) FROM MedicalRecords m WHERE m.IsActive = TRUE AND m.PaymentStatus = 'Paid'), 0) +
                        COALESCE((SELECT SUM(o.TotalDiscountedPrice) FROM Orders o WHERE o.IsActive = TRUE AND o.Status = 'Paid'), 0)
                    ) AS TotalRevenue,
                    (SELECT MAX(e.CreatedAt) FROM EventLogs e WHERE e.IsActive = TRUE) AS LastActivity
                FROM Clinics c
                WHERE c.IsActive = TRUE
                LIMIT 1;

            ";

                var result = await _db.QueryFirstOrDefaultAsync<ClinicReportsClientDTO>(query);
                return result;
            }
        }

        public async Task<IEnumerable<ClinicProductUsageDTO>> GetClinicProductUsageReportsAsync(string db)
        {
            using (var _db = _dbFactory.GetDbConnection(db))
            {
                const string query = @"
                    SELECT
                      p.Id                 AS ProductId,
                      p.Name               AS ProductName,
                      p.Alias              AS ProductAliasName,
                      pc.Name              AS ProductCategory,
                      CONCAT(COALESCE(ps.Volume,''), ' ', COALESCE(ps.VolumeUnit,'')) AS ProductVolume,
                      AVG(COALESCE(mrp.Price, p.Price, 0)) AS Price,
                      SUM(COALESCE(mrp.Quantity, 1)) AS TotalUsage
                    FROM MedicalRecordsPrescriptions mrp
                    JOIN MedicalRecords mr ON mr.Id = mrp.MedicalRecordsId
                    JOIN Appointments a ON a.Id = mr.AppointmentId
                    JOIN Products p ON p.Id = mrp.ProductId
                    LEFT JOIN ProductCategories pc ON pc.Id = p.CategoryId AND pc.IsActive = b'1'
                    LEFT JOIN ProductStocks ps ON ps.Id = (
                      SELECT ps2.Id
                      FROM ProductStocks ps2
                      WHERE ps2.ProductId = p.Id AND ps2.IsActive = b'1'
                      ORDER BY ps2.Id DESC
                      LIMIT 1
                    )
                    WHERE
                      mrp.IsActive = b'1'
                      AND mr.IsActive = b'1'
                      AND a.IsActive = b'1'
                      AND a.StatusId = 6
                      AND mrp.Type = 'Product'
                      AND p.IsActive = b'1'
                    GROUP BY
                      p.Id, p.Name, p.Alias, pc.Name, ps.Volume, ps.VolumeUnit
                    ORDER BY TotalUsage DESC;
            ";

                var result = await _db.QueryAsync<ClinicProductUsageDTO>(query);
                return result;
            }
        }

        public async Task<IEnumerable<ClinicAnimalUsageDTO>> GetClinicAnimalUsageReportsAsync(string db)
        {
            using (var _db = _dbFactory.GetDbConnection(db))
            {
                const string query = @"
                   SELECT
                      a.Id AS AnimalId,
                      a.Name AS Name,
                      COUNT(p.Id) AS `Count`
                    FROM Animals a
                    LEFT JOIN Patients p
                      ON TRIM(LOWER(p.Species)) = TRIM(LOWER(a.Name))
                      AND p.IsActive = b'1'
                    WHERE
                      a.IsActive = b'1'
                    GROUP BY
                      a.Id,
                      a.Name
                    ORDER BY
                      `Count` DESC;
            ";

                var result = await _db.QueryAsync<ClinicAnimalUsageDTO>(query);
                return result;
            }
        }
    }
}
