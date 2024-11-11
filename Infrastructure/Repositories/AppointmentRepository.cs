using Dapper;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using System.Globalization;

namespace Infrastructure.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointments, AppointmentsFilter>, IAppointmentRepository
    {
        protected readonly string _tableStatus;

        public AppointmentRepository(IDBFactory context) : base(context)
        {
            _tableStatus = typeof(AppointmentsStatus).Name;
        }

        public async Task AddStatusRange(IEnumerable<AppointmentsStatus> entities, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            foreach (var item in entities)
            {
                var propertyNames = QueryGenerator.GetPropertyNames(item);
                var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
                var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

                var subquery = $"INSERT INTO {_tableStatus} ({columnNames}) VALUES ({parameterNames}) ";
                await _db.ExecuteAsync(subquery, item);
            }
        }

        public async Task<IEnumerable<BookingHistoryResponse>> GetBookingHistoryOwner(string dbName, int ownerId)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var data = await _db.QueryAsync<BookingHistoryResponse>($@"SELECT a.Id AS AppointmentId, a.OwnersId AS OwnerId, o.Name AS OwnerName, o.Title AS OwnerTitle, a.PatientsId AS PatientId, p.Name AS PatientName, 
                 p.Species AS PatientSpecies, p.Breed AS PatientBreed, mr.Id AS MedicalRecordsId, a.ServiceId, COALESCE(s.Name, a.Type) AS ServiceName, a.StaffId, pr.Name AS StaffName, 
                 st.Name AS StatusName, mr.PaymentStatus AS StatusPayment, a.Date AS DateAppointment,
                 mr.StartDate AS StartDate, mr.EndDate AS EndDate, COALESCE(mr.Total, 0) AS TotalPrice, mr.Code
                 FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId 
                 JOIN Patients p ON p.Id = a.PatientsId 
                 LEFT JOIN Services s ON s.Id = a.ServiceId 
                 JOIN Profile pr ON pr.Id = a.StaffId 
                 JOIN AppointmentsStatus st ON st.Id = a.StatusId
                 Left JOIN MedicalRecords mr ON mr.AppointmentId = a.Id
                WHERE a.OwnersId = @ownerId", new { ownerId = ownerId });
            return data;
        }

        public async Task<IEnumerable<BookingHistoryResponse>> GetBookingHistoryPatient(string dbName, int patientId)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var data = await _db.QueryAsync<BookingHistoryResponse>($@"SELECT a.Id AS AppointmentId, a.OwnersId AS OwnerId, o.Name AS OwnerName, o.Title AS OwnerTitle, a.PatientsId AS PatientId, p.Name AS PatientName, 
                 p.Species AS PatientSpecies, p.Breed AS PatientBreed, mr.Id AS MedicalRecordsId, a.ServiceId, COALESCE(s.Name, a.Type) AS ServiceName, a.StaffId, pr.Name AS StaffName, 
                 st.Name AS StatusName, mr.PaymentStatus AS StatusPayment, a.Date AS DateAppointment,
                 mr.StartDate AS StartDate, mr.EndDate AS EndDate, COALESCE(mr.Total, 0)AS TotalPrice, mr.Code
                 FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId 
                 JOIN Patients p ON p.Id = a.PatientsId 
                 Left JOIN Services s ON s.Id = a.ServiceId 
                 JOIN Profile pr ON pr.Id = a.StaffId 
                 JOIN AppointmentsStatus st ON st.Id = a.StatusId
                 JOIN MedicalRecords mr ON mr.AppointmentId = a.Id
                WHERE a.PatientsId = @patientId", new { patientId = patientId });
            return data;
        }

        public async Task<DataResultDTO<AppointmentsDetailResponse>> GetAllDetailList(string dbName, AppointmentDetailFilter filter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            // Start building the SQL query
            var sqlQuery = $@"SELECT a.Id AS AppointmentId, COALESCE(mr.Id, 0) AS MedicalRecordId, a.OwnersId, o.Name AS OwnersName, o.Title AS OwnersTitle, a.PatientsId, p.Name AS PatientsName, p.Breed AS PatientsBreed, 
                a.ServiceId, COALESCE(s.Name, a.Type) AS ServiceName, a.StaffId, pr.Name AS StaffName, a.StatusId, st.Name AS StatusName, a.Notes, a.Date, s.Duration AS DurationEstimate, 
                s.DurationType AS DurationTypeEstimate, 
                CASE 
        WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE) 
        WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR) 
        WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY) 
        WHEN s.DurationType IS NULL THEN a.Date
        ELSE NULL 
    END AS EndDateEstimate, s.Price AS Total,
                CASE 
                    WHEN op.MedicalRecordId IS NOT NULL THEN TRUE 
                    ELSE FALSE 
                END AS IsOpname,
                CASE
                    WHEN a.Type IS NOT NULL THEN a.Type
                    WHEN a.ServiceId IS NOT NULL AND a.Type IS NULL THEN s.Name
                    ELSE NULL
                END AS Type
                FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId 
                JOIN Patients p ON p.Id = a.PatientsId 
                LEFT JOIN Services s ON s.Id = a.ServiceId 
                JOIN Profile pr ON pr.Id = a.StaffId 
                JOIN AppointmentsStatus st ON st.Id = a.StatusId
                LEFT JOIN MedicalRecords mr ON mr.AppointmentId = a.Id
                LEFT JOIN (
                    SELECT DISTINCT MedicalRecordId
                    FROM OpnamePatients
                ) op ON op.MedicalRecordId = mr.Id ";
            if (filter != null)
            {
                var whereClause = new List<string>();

                // Check and add StatusId filter
                if (filter.StatusId.HasValue)
                {
                    whereClause.Add($"a.StatusId = {filter.StatusId.Value}");
                }

                // Check and add StaffId filter
                if (filter.StaffId.HasValue)
                {
                    whereClause.Add($"a.StaffId = {filter.StaffId.Value}");
                }

                // Check and add Date filter
                if (!string.IsNullOrEmpty(filter.Date))
                {
                    // Parse the date range if it's in the format '[start] - [end]'
                    if (filter.Date.Contains("-"))
                    {
                        var dateRangeParts = filter.Date.Split('-');
                        if (dateRangeParts.Length == 2)
                        {
                            string startDateStr = dateRangeParts[0].Trim();
                            string endDateStr = dateRangeParts[1].Trim();

                            DateTime startDate, endDate;
                            if (DateTime.TryParseExact(startDateStr, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate) &&
                                DateTime.TryParseExact(endDateStr, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                            {
                                whereClause.Add($"DATE(a.Date) >= '{startDate:yyyy-MM-dd}' AND DATE(a.Date) <= '{endDate:yyyy-MM-dd}'");
                            }
                        }
                    }
                    else // Single date case
                    {
                        if (DateTime.TryParseExact(filter.Date, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                        {
                            whereClause.Add($"DATE(a.Date) = '{date:yyyy-MM-dd}'");
                        }
                    }
                }

                // Combine all where conditions
                if (whereClause.Count > 0)
                {
                    sqlQuery += " WHERE " + string.Join(" AND ", whereClause);
                }
            }
            // Execute the SQL query
            var data = await _db.QueryAsync<AppointmentsDetailResponse>(sqlQuery);

            var result = new DataResultDTO<AppointmentsDetailResponse>
            {
                Data = data,
                TotalData = data.Count()
            };
            return result;
        }

        public async Task<IEnumerable<AppointmentsDetailResponse>> GetAllDetailListToday(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            DateTime currentDate = DateTime.Now.Date; // Get the current date with time component set to midnight (00:00:00)
            return await _db.QueryAsync<AppointmentsDetailResponse>($@"SELECT a.Id AS AppointmentId, a.OwnersId, COALESCE(mr.Id, 0) AS MedicalRecordId, o.Name AS OwnersName, o.Title AS OwnersTitle, a.PatientsId, p.Name AS PatientsName, p.Breed AS PatientsBreed, 
                a.ServiceId, COALESCE(s.Name, a.Type) AS ServiceName, a.StaffId, pr.Name AS StaffName, a.StatusId, st.Name AS StatusName, a.Notes, a.Date, s.Duration AS DurationEstimate, 
                s.DurationType AS DurationTypeEstimate, 
                CASE 
        WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE) 
        WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR) 
        WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY) 
        WHEN s.DurationType IS NULL THEN a.Date
        ELSE NULL 
    END AS EndDateEstimate, s.Price AS Total,
                CASE 
                    WHEN op.MedicalRecordId IS NOT NULL THEN TRUE 
                    ELSE FALSE 
                END AS IsOpname,
                CASE
                    WHEN a.Type IS NOT NULL THEN a.Type
                    WHEN a.ServiceId IS NOT NULL AND a.Type IS NULL THEN s.Name
                    ELSE NULL
                END AS Type
                FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId 
                JOIN Patients p ON p.Id = a.PatientsId 
                LEFT JOIN Services s ON s.Id = a.ServiceId 
                JOIN Profile pr ON pr.Id = a.StaffId 
                JOIN AppointmentsStatus st ON st.Id = a.StatusId 
                LEFT JOIN MedicalRecords mr ON mr.AppointmentId = a.Id 
                LEFT JOIN (
                    SELECT DISTINCT MedicalRecordId
                    FROM OpnamePatients
                ) op ON op.MedicalRecordId = mr.Id 
                WHERE DATE(a.Date) = @CurrentDate", new { CurrentDate = currentDate });
        }

        public async Task<AppointmentsDetailResponse> GetAllDetail(int id, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryFirstAsync<AppointmentsDetailResponse>($@"SELECT a.Id AS AppointmentId, a.OwnersId, o.Name AS OwnersName, o.Title AS OwnersTitle, a.PatientsId, p.Name AS PatientsName, p.Breed AS PatientsBreed, 
                a.ServiceId, COALESCE(s.Name, a.Type) AS ServiceName, a.StaffId, pr.Name AS StaffName, a.StatusId, st.Name AS StatusName, a.Notes, a.Date, s.Duration AS DurationEstimate, 
                s.DurationType AS DurationTypeEstimate, COALESCE(mr.Id, 0)  AS MedicalRecordId,
                CASE 
        WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE) 
        WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR) 
        WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY) 
        WHEN s.DurationType IS NULL THEN a.Date
        ELSE NULL 
    END AS EndDateEstimate, s.Price AS Total,
                CASE 
                    WHEN op.MedicalRecordId IS NOT NULL THEN TRUE 
                    ELSE FALSE 
                END AS IsOpname,
                CASE
                    WHEN a.Type IS NOT NULL THEN a.Type
                    WHEN a.ServiceId IS NOT NULL AND a.Type IS NULL THEN s.Name
                    ELSE NULL
                END AS Type
                FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId 
                JOIN Patients p ON p.Id = a.PatientsId 
                LEFT JOIN Services s ON s.Id = a.ServiceId 
                JOIN Profile pr ON pr.Id = a.StaffId 
                JOIN AppointmentsStatus st ON st.Id = a.StatusId 
                LEFT JOIN MedicalRecords AS mr ON mr.AppointmentId = a.Id 
                LEFT JOIN (
                    SELECT DISTINCT MedicalRecordId
                    FROM OpnamePatients
                ) op ON op.MedicalRecordId = mr.Id 
                WHERE a.Id = @id", new { id = id });
        }

        public async Task<IEnumerable<AppointmentsStatus>> GetAllStatus(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<AppointmentsStatus>($"SELECT * FROM AppointmentsStatus");
        }

        public async Task<int> AddActivity(AppointmentsActivity entity, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(entity);

            var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
            var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

            var query = $"INSERT INTO AppointmentsActivity ({columnNames}) VALUES ({parameterNames}); SELECT LAST_INSERT_ID();";
            return await _db.ExecuteScalarAsync<int>(query, entity);
        }

        public async Task<IEnumerable<Appointments>> GetAllByStatusId(string dbName, int statusId)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<Appointments>($"SELECT * FROM Appointments WHERE StatusId = @Id", new { Id = statusId });
        }

        public async Task<DataResultDTO<AppointmentMedicalDetailResponse>> GetAllDetailMedicalList(string dbName, AppointmentDetailFilter filter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            // Start building the SQL query
            var sqlQuery = $@"SELECT a.Id AS AppointmentId, COALESCE(mr.Id, 0) AS MedicalRecordId, a.OwnersId, o.Name AS OwnersName, o.Title AS OwnersTitle, a.PatientsId, p.Name AS PatientsName, p.Breed AS PatientsBreed, 
                a.ServiceId, COALESCE(s.Name, a.Type) AS ServiceName, a.StaffId, pr.Name AS StaffName, a.StatusId, st.Name AS StatusName, a.Notes, a.Date, s.Duration AS DurationEstimate, 
                s.DurationType AS DurationTypeEstimate, 
                CASE 
        WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE) 
        WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR) 
        WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY) 
        WHEN s.DurationType IS NULL THEN a.Date
        ELSE NULL 
    END AS EndDateEstimate, s.Price AS Total,
                CASE 
                    WHEN op.MedicalRecordId IS NOT NULL THEN TRUE 
                    ELSE FALSE 
                END AS IsOpname,
                CASE
                    WHEN a.Type IS NOT NULL THEN a.Type
                    WHEN a.ServiceId IS NOT NULL AND a.Type IS NULL THEN s.Name
                    ELSE NULL
                END AS Type
                FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId 
                JOIN Patients p ON p.Id = a.PatientsId 
                LEFT JOIN Services s ON s.Id = a.ServiceId 
                JOIN Profile pr ON pr.Id = a.StaffId 
                JOIN AppointmentsStatus st ON st.Id = a.StatusId
                LEFT JOIN MedicalRecords mr ON mr.AppointmentId = a.Id
                LEFT JOIN (
                    SELECT DISTINCT MedicalRecordId
                    FROM OpnamePatients
                ) op ON op.MedicalRecordId = mr.Id 
                WHERE 
                    COALESCE(mr.Id, 0) != 0";
            if (filter != null)
            {
                var whereClause = new List<string>();

                // Check and add StatusId filter
                if (filter.StatusId.HasValue)
                {
                    whereClause.Add($"a.StatusId = {filter.StatusId.Value}");
                }

                // Check and add StaffId filter
                if (filter.StaffId.HasValue)
                {
                    whereClause.Add($"a.StaffId = {filter.StaffId.Value}");
                }

                // Check and add Date filter
                if (!string.IsNullOrEmpty(filter.Date))
                {
                    // Parse the date range if it's in the format '[start] - [end]'
                    if (filter.Date.Contains("-"))
                    {
                        var dateRangeParts = filter.Date.Split('-');
                        if (dateRangeParts.Length == 2)
                        {
                            string startDateStr = dateRangeParts[0].Trim();
                            string endDateStr = dateRangeParts[1].Trim();

                            DateTime startDate, endDate;
                            if (DateTime.TryParseExact(startDateStr, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate) &&
                                DateTime.TryParseExact(endDateStr, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                            {
                                whereClause.Add($"DATE(a.Date) >= '{startDate:yyyy-MM-dd}' AND DATE(a.Date) <= '{endDate:yyyy-MM-dd}'");
                            }
                        }
                    }
                    else // Single date case
                    {
                        if (DateTime.TryParseExact(filter.Date, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                        {
                            whereClause.Add($"DATE(a.Date) = '{date:yyyy-MM-dd}'");
                        }
                    }
                }

                // Combine all where conditions
                if (whereClause.Count > 0)
                {
                    sqlQuery += " AND " + string.Join(" AND ", whereClause);
                }
            }
            // Execute the SQL query
            var data = await _db.QueryAsync<AppointmentMedicalDetailResponse>(sqlQuery);

            var result = new DataResultDTO<AppointmentMedicalDetailResponse>
            {
                Data = data,
                TotalData = data.Count()
            };
            return result;
        }
    }
}
