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
    }
}
