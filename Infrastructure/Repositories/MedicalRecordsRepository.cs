using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Xml.Linq;

namespace Infrastructure.Repositories
{
    public class MedicalRecordsRepository : GenericRepository<MedicalRecords, MedicalRecordsFilter>, IMedicalRecordsRepository
    {
        public MedicalRecordsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<MedicalRecords> GetByAppointmentId(string dbName, int appointmentId)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var data = await _db.QueryFirstAsync<MedicalRecords>($"SELECT * FROM MedicalRecords WHERE AppointmentId = @Id", new { Id = appointmentId });
            return data;
        }

        public async Task<MedicalRecordsDetailResponse> GetDetailById(string dbName, int medicalRecordId, string flag)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var sql = @"
                -- Medical Record
                SELECT * FROM MedicalRecords WHERE Id = @medicalRecordId;
        
                -- Appointment
                SELECT * FROM Appointments WHERE Id = (SELECT AppointmentId FROM MedicalRecords WHERE Id = @medicalRecordId);

                -- Service
                SELECT * FROM Services WHERE Id = (SELECT ServiceId FROM Appointments WHERE Id = (SELECT AppointmentId FROM MedicalRecords WHERE Id = @medicalRecordId));

                -- Staff
                SELECT * FROM Profile WHERE Id = (SELECT StaffId FROM MedicalRecords WHERE Id = @medicalRecordId);

                -- Patient and Owner
                SELECT * FROM Patients WHERE Id = (SELECT PatientId FROM MedicalRecords WHERE Id = @medicalRecordId);
                SELECT * FROM Owners WHERE Id = (SELECT OwnersId FROM Patients WHERE Id = (SELECT PatientId FROM MedicalRecords WHERE Id = @medicalRecordId));

                -- Notes, Diagnoses, Prescriptions (only if flag is not 'no_notes')
                " + (flag != "no_notes" ? "SELECT * FROM MedicalRecordsNotes WHERE MedicalRecordsId = @medicalRecordId;" : "") + @"
                SELECT * FROM MedicalRecordsDiagnoses WHERE MedicalRecordsId = @medicalRecordId;
                SELECT * FROM MedicalRecordsPrescriptions WHERE MedicalRecordsId = @medicalRecordId;

                -- Payments
                SELECT * FROM OrdersPayment WHERE OrderId = @medicalRecordId AND Type = 'MedicalRecord';

                -- Opname and Opname Patients
                SELECT * FROM OpnamePatients WHERE MedicalRecordId = @medicalRecordId;
                SELECT * FROM Opnames WHERE Id = (SELECT OpnameId FROM OpnamePatients WHERE MedicalRecordId = @medicalRecordId LIMIT 1);
            ";

            using var multi = await _db.QueryMultipleAsync(sql, new { medicalRecordId });

            var medicalRecord = await multi.ReadSingleOrDefaultAsync<MedicalRecords>();
            if (medicalRecord == null) return null;

            var appointment = await multi.ReadSingleOrDefaultAsync<Appointments>();
            var service = await multi.ReadSingleOrDefaultAsync<Services>();
            var staff = await multi.ReadSingleOrDefaultAsync<Profile>();
            var patient = await multi.ReadSingleOrDefaultAsync<Patients>();
            var owner = await multi.ReadSingleOrDefaultAsync<Owners>();

            IEnumerable<MedicalRecordsNotes> notes = null;
            if (flag != "no_notes")
                notes = await multi.ReadAsync<MedicalRecordsNotes>();

            var diagnoses = await multi.ReadAsync<MedicalRecordsDiagnoses>();
            var prescriptions = await multi.ReadAsync<MedicalRecordsPrescriptions>();

            var payments = await multi.ReadAsync<OrdersPayment>();
            var totalLastPayment = payments.Sum(p => p.Total);
            string statusPayment = payments.Any() ? (totalLastPayment < medicalRecord.Total ? "Paid Less" : "Paid") : "Unpaid";

            var opnamePatients = await multi.ReadAsync<OpnamePatients>();
            var dataOpnamePatient = opnamePatients.FirstOrDefault();
            OpnameDetailResponse opnameDetail = null;

            if (dataOpnamePatient != null)
            {
                var opname = await multi.ReadSingleOrDefaultAsync<Opnames>();
                opnameDetail = new OpnameDetailResponse
                {
                    OpnameName = opname?.Name,
                    StartTime = dataOpnamePatient.StartTime,
                    EstimatedDays = dataOpnamePatient.EstimateDays,
                    EndTime = dataOpnamePatient.EndTime,
                    OpnameId = dataOpnamePatient.OpnameId,
                    OpnamePatientsId = dataOpnamePatient.Id,
                    Price = dataOpnamePatient.Price,
                    Status = dataOpnamePatient.Status,
                    TotalPrice = dataOpnamePatient.TotalPrice
                };
            }

            return new MedicalRecordsDetailResponse
            {
                Id = medicalRecord.Id,
                Code = medicalRecord.Code,
                Appointments = appointment,
                Patients = patient,
                Services = service,
                Owners = owner,
                Staff = staff,
                StartDate = medicalRecord.StartDate,
                DiscountMethod = medicalRecord.DiscountMethod,
                DiscountValue = medicalRecord.DiscountValue,
                DiscountTotal = medicalRecord.DiscountTotal,
                TotalDiscounted = medicalRecord.TotalDiscounted,
                EndDate = medicalRecord.EndDate ?? DateTime.MinValue,
                TotalPrice = medicalRecord.Total,
                TotalPaid = totalLastPayment,
                Prescriptions = prescriptions,
                Diagnoses = diagnoses,
                StatusPayment = statusPayment,
                Notes = notes,
                OpnameDetail = opnameDetail
            };
        }

        public async Task<string> GetLatestCode(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            string query = "SELECT Code FROM MedicalRecords ORDER BY Id DESC";
            return await _db.QueryFirstOrDefaultAsync<string>(query);
        }

        public async Task<RevenueDataResponse> GetSalesDetail(string dbName, string query)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var filter = "";
            if (!string.IsNullOrEmpty(query))
            {
                filter = $"And {query}";
            }
            string queryFinal = $"SELECT SUM(mrp.Total), SUM(CASE WHEN mrp.Type = 'Product' THEN mrp.Total ELSE 0 END) AS ProductsTotal, SUM(CASE WHEN mrp.Type = 'Service' THEN mrp.Total ELSE 0 END) AS ServicesTotal " +
                $"FROM MedicalRecords mr " +
                $"JOIN MedicalRecordsPrescriptions mrp ON mrp.MedicalRecordsId = mr.Id " +
                $"WHERE mr.PaymentStatus = 'Paid' {filter}";
            return await _db.QueryFirstOrDefaultAsync<RevenueDataResponse>(queryFinal);
        }

        public async Task<IEnumerable<MonthlyDataChart>> GetTotalMedicalSales(string dbName, string dateFilter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var filterQuery = "YEAR(mr.CreatedAt) = YEAR(CURRENT_DATE()) AND mr.CreatedAt <= CURRENT_DATE()";
            if (dateFilter != null)
            {
                filterQuery = dateFilter;
            }
            string query = @"SELECT
                                DATE_FORMAT(mr.CreatedAt, '%d') AS Date,
                                DATE_FORMAT(mr.CreatedAt, '%m') AS Month,
                                DATE_FORMAT(mr.CreatedAt, '%Y') AS Year,
                                SUM(mrp.Total) AS Total
                            FROM
                              MedicalRecords mr
                            JOIN 
                              MedicalRecordsPrescriptions mrp ON mrp.MedicalRecordsId = mr.Id
                            WHERE
                              mr.PaymentStatus = 'Paid' AND 
            ";
            query += filterQuery;
            query += " GROUP BY Date, Month, Year ORDER BY Date, Year, Month;";
            return await _db.QueryAsync<MonthlyDataChart>(query);
        }

        public async Task<IEnumerable<MonthlyDataChart>> GetVisitYearly(string dbName, string? dateFilter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var filterQuery = "YEAR(CreatedAt) = YEAR(CURRENT_DATE()) AND CreatedAt <= CURRENT_DATE()";
            if(dateFilter != null)
            {
                filterQuery = dateFilter;
            }
            string query = @"SELECT
                                DATE_FORMAT(CreatedAt, '%d') AS Date,
                                DATE_FORMAT(CreatedAt, '%m') AS Month,
                                DATE_FORMAT(CreatedAt, '%Y') AS Year,
                                COUNT(*) AS Total
                            FROM
                                MedicalRecords
                            WHERE 
            ";
            query += filterQuery;
            query += " GROUP BY Date, Month, Year ORDER BY Date, Year, Month;";
            return await _db.QueryAsync<MonthlyDataChart>(query);
        }
    }
}
