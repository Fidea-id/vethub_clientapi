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

        private static bool TryBuildDateRange(string rawDate, out DateTime startDate, out DateTime endExclusive)
        {
            startDate = default;
            endExclusive = default;

            if (string.IsNullOrWhiteSpace(rawDate))
            {
                return false;
            }

            var normalized = rawDate.Trim()
                .Replace('–', '-')
                .Replace('—', '-');

            var parts = normalized.Split(new[] { '-' }, 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                if (TryParseFilterDate(parts[0], out var start) &&
                    TryParseFilterDate(parts[1], out var end))
                {
                    startDate = start.Date;
                    endExclusive = end.Date.AddDays(1);
                    return true;
                }
            }
            else if (TryParseFilterDate(normalized, out var singleDate))
            {
                startDate = singleDate.Date;
                endExclusive = startDate.AddDays(1);
                return true;
            }

            return false;
        }

        private static bool TryParseFilterDate(string value, out DateTime parsedDate)
        {
            var formats = new[]
            {
                "d MMMM yyyy",
                "dd MMMM yyyy",
                "d MMM yyyy",
                "dd MMM yyyy",
                "d/M/yyyy",
                "dd/MM/yyyy",
                "d-M-yyyy",
                "dd-MM-yyyy",
                "yyyy-MM-dd"
            };

            return DateTime.TryParseExact(
                       value.Trim(),
                       formats,
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.AllowWhiteSpaces,
                       out parsedDate)
                   || DateTime.TryParse(
                       value.Trim(),
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.AllowWhiteSpaces,
                       out parsedDate)
                   || DateTime.TryParse(
                       value.Trim(),
                       CultureInfo.CurrentCulture,
                       DateTimeStyles.AllowWhiteSpaces,
                       out parsedDate);
        }

        private static void ApplyDetailFilters(AppointmentDetailFilter filter, string dateColumn, List<string> whereClause, DynamicParameters parameters)
        {
            if (filter == null)
            {
                return;
            }

            if (filter.StatusId.HasValue)
            {
                whereClause.Add("a.StatusId = @StatusId");
                parameters.Add("StatusId", filter.StatusId.Value);
            }

            if (filter.StaffId.HasValue)
            {
                whereClause.Add("a.StaffId = @StaffId");
                parameters.Add("StaffId", filter.StaffId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                whereClause.Add("(mr.Code LIKE @Search OR p.Name LIKE @Search OR o.Name LIKE @Search OR COALESCE(s.Name, a.Type) LIKE @Search)");
                parameters.Add("Search", $"%{filter.Search.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(filter.Date) && TryBuildDateRange(filter.Date, out var startDate, out var endExclusive))
            {
                whereClause.Add($"{dateColumn} >= @StartDate AND {dateColumn} < @EndDate");
                parameters.Add("StartDate", startDate);
                parameters.Add("EndDate", endExclusive);
            }
        }

        private static string BuildAppointmentDetailFromClause(bool includeActivityInfo, bool medicalOnly = false)
        {
            var medicalJoin = medicalOnly
                ? "INNER JOIN MedicalRecords mr ON mr.AppointmentId = a.Id"
                : "LEFT JOIN MedicalRecords mr ON mr.AppointmentId = a.Id";

            var activityJoins = includeActivityInfo
                ? @"
                        LEFT JOIN (
                            SELECT AppointmentId, COUNT(*) AS MedicalRecordCount
                            FROM AppointmentsActivity
                            WHERE CurrentStatusId = 3
                            GROUP BY AppointmentId
                        ) acts ON acts.AppointmentId = a.Id
                        LEFT JOIN (
                            SELECT AppointmentId, MAX(CurrentDate) AS InvoiceDate
                            FROM AppointmentsActivity
                            WHERE CurrentStatusId = 6
                            GROUP BY AppointmentId
                        ) act ON act.AppointmentId = a.Id"
                : string.Empty;

            return $@"FROM Appointments a
                        JOIN Owners o ON o.Id = a.OwnersId
                        JOIN Patients p ON p.Id = a.PatientsId
                        LEFT JOIN Services s ON s.Id = a.ServiceId
                        LEFT JOIN AppointmentsType at ON at.Name = a.Type
                        JOIN Profile pr ON pr.Id = a.StaffId
                        JOIN AppointmentsStatus st ON st.Id = a.StatusId
                        {medicalJoin}{activityJoins}";
        }

        private static string BuildAppointmentDetailSelect(bool includeActivityInfo, bool medicalOnly = false)
        {
            var activityColumns = includeActivityInfo
                ? @",
                               CASE
                                   WHEN acts.MedicalRecordCount > 1 THEN TRUE
                                   ELSE FALSE
                               END AS IsEdit,
                               act.InvoiceDate"
                : string.Empty;

            return $@"SELECT a.Id AS AppointmentId,
                               COALESCE(mr.Id, 0) AS MedicalRecordId,
                               a.OwnersId,
                               o.Name AS OwnersName,
                               o.Title AS OwnersTitle,
                               a.PatientsId,
                               p.Name AS PatientsName,
                               p.Breed AS PatientsBreed,
                               a.ServiceId,
                               COALESCE(s.Name, a.Type) AS ServiceName,
                               at.Color AS TypeColor,
                               a.StaffId,
                               pr.Name AS StaffName,
                               a.StatusId,
                               st.Name AS StatusName,
                               a.Notes,
                               a.Date,
                               s.Duration AS DurationEstimate,
                               s.DurationType AS DurationTypeEstimate,
                               CASE
                                   WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE)
                                   WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR)
                                   WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY)
                                   WHEN s.DurationType IS NULL THEN a.Date
                                   ELSE NULL
                               END AS EndDateEstimate,
                               s.Price AS Total,
                               CASE
                                   WHEN EXISTS (
                                       SELECT 1
                                       FROM OpnamePatients op
                                       WHERE op.MedicalRecordId = mr.Id
                                       LIMIT 1
                                   ) THEN TRUE
                                   ELSE FALSE
                               END AS IsOpname,
                               CASE
                                   WHEN a.Type IS NOT NULL THEN a.Type
                                   WHEN a.ServiceId IS NOT NULL AND a.Type IS NULL THEN s.Name
                                   ELSE NULL
                               END AS Type,
                               a.CreatedAt AS CreatedAt{activityColumns}
                        {BuildAppointmentDetailFromClause(includeActivityInfo, medicalOnly)}";
        }

        public async Task AddStatusRange(IEnumerable<AppointmentsStatus> entities, string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                foreach (var item in entities)
                {
                    var propertyNames = QueryGenerator.GetPropertyNames(item);
                    var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
                    var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

                    var subquery = $"INSERT INTO {_tableStatus} ({columnNames}) VALUES ({parameterNames}) ";
                    await _db.ExecuteAsync(subquery, item);
                }
            }
        }

        public async Task<IEnumerable<BookingHistoryResponse>> GetBookingHistoryOwner(string dbName, int ownerId)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var data = await _db.QueryAsync<BookingHistoryResponse>($@"SELECT a.Id AS AppointmentId, a.OwnersId AS OwnerId, o.Name AS OwnerName, o.Title AS OwnerTitle, a.PatientsId AS PatientId, p.Name AS PatientName, 
                     p.Species AS PatientSpecies, p.Breed AS PatientBreed, mr.Id AS MedicalRecordsId, a.ServiceId, COALESCE(s.Name, a.Type) AS ServiceName, a.StaffId, pr.Name AS StaffName, 
                     st.Name AS StatusName, mr.PaymentStatus AS StatusPayment, a.Date AS DateAppointment,
                     mr.StartDate AS StartDate, mr.EndDate AS EndDate, COALESCE(mr.Total, 0) AS TotalPrice, mr.Code
                     FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId 
                     JOIN Patients p ON p.Id = a.PatientsId 
                     LEFT JOIN Services s ON s.Id = a.ServiceId 
                     LEFT JOIN AppointmentsType at ON at.Name = a.Type
                     JOIN Profile pr ON pr.Id = a.StaffId 
                     JOIN AppointmentsStatus st ON st.Id = a.StatusId
                     Left JOIN MedicalRecords mr ON mr.AppointmentId = a.Id
                    WHERE a.OwnersId = @ownerId AND a.IsActive = true ", new { ownerId = ownerId });
                return data;
            }
        }

        public async Task<IEnumerable<BookingHistoryResponse>> GetBookingHistoryPatient(string dbName, int patientId)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var data = await _db.QueryAsync<BookingHistoryResponse>($@"SELECT a.Id AS AppointmentId, a.OwnersId AS OwnerId, o.Name AS OwnerName, o.Title AS OwnerTitle, a.PatientsId AS PatientId, p.Name AS PatientName, 
                     p.Species AS PatientSpecies, p.Breed AS PatientBreed, mr.Id AS MedicalRecordsId, a.ServiceId, COALESCE(s.Name, a.Type) AS ServiceName, a.StaffId, pr.Name AS StaffName, 
                     st.Name AS StatusName, mr.PaymentStatus AS StatusPayment, a.Date AS DateAppointment,
                     mr.StartDate AS StartDate, mr.EndDate AS EndDate, COALESCE(mr.Total, 0)AS TotalPrice, mr.Code
                     FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId 
                     JOIN Patients p ON p.Id = a.PatientsId 
                     Left JOIN Services s ON s.Id = a.ServiceId 
                     LEFT JOIN AppointmentsType at ON at.Name = a.Type
                     JOIN Profile pr ON pr.Id = a.StaffId 
                     JOIN AppointmentsStatus st ON st.Id = a.StatusId
                     JOIN MedicalRecords mr ON mr.AppointmentId = a.Id
                    WHERE a.PatientsId = @patientId AND a.IsActive = true ", new { patientId = patientId });
                return data;
            }
        }

        public async Task<DataResultDTO<AppointmentsDetailResponse>> GetAllDetailList(string dbName, AppointmentDetailFilter filter)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var sqlQuery = BuildAppointmentDetailSelect(includeActivityInfo: false);
                var countQuery = $"SELECT COUNT(1) {BuildAppointmentDetailFromClause(includeActivityInfo: false)}";
                var whereClause = new List<string> { "a.IsActive = true" };
                var parameters = new DynamicParameters();

                ApplyDetailFilters(filter, "a.Date", whereClause, parameters);

                var whereSql = " WHERE " + string.Join(" AND ", whereClause);
                var totalData = await _db.ExecuteScalarAsync<int>(countQuery + whereSql, parameters);

                sqlQuery += whereSql;
                sqlQuery += " ORDER BY a.Date DESC, a.Id DESC";

                if (filter?.Take > 0)
                {
                    sqlQuery += " LIMIT @Take OFFSET @Skip";
                    parameters.Add("Take", filter.Take);
                    parameters.Add("Skip", filter.Skip);
                }

                var data = await _db.QueryAsync<AppointmentsDetailResponse>(sqlQuery, parameters);

                var result = new DataResultDTO<AppointmentsDetailResponse>
                {
                    Data = data,
                    TotalData = totalData
                };
                return result;
            }
        }

        public async Task<DataResultDTO<AppointmentsDetailResponse>> GetPagedDetailList(string dbName, AppointmentDetailFilter filter)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var sqlQuery = BuildAppointmentDetailSelect(includeActivityInfo: true);
                var countQuery = $"SELECT COUNT(1) {BuildAppointmentDetailFromClause(includeActivityInfo: false)}";
                var whereClause = new List<string> { "a.IsActive = true" };
                var parameters = new DynamicParameters();

                ApplyDetailFilters(filter, "a.Date", whereClause, parameters);

                var whereSql = " WHERE " + string.Join(" AND ", whereClause);
                var totalData = await _db.ExecuteScalarAsync<int>(countQuery + whereSql, parameters);

                sqlQuery += whereSql;
                sqlQuery += " ORDER BY a.Date DESC, a.Id DESC";

                if (filter?.Take > 0)
                {
                    sqlQuery += " LIMIT @Take OFFSET @Skip";
                    parameters.Add("Take", filter.Take);
                    parameters.Add("Skip", filter.Skip);
                }

                var data = await _db.QueryAsync<AppointmentsDetailResponse>(sqlQuery, parameters);

                return new DataResultDTO<AppointmentsDetailResponse>
                {
                    Data = data,
                    TotalData = totalData
                };
            }
        }

        public async Task<DataResultDTO<AppointmentsDetailResponse>> GetLastDetailList(string dbName, AppointmentDetailFilter filter, int take)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var sqlQuery = BuildAppointmentDetailSelect(includeActivityInfo: false);
                var whereClause = new List<string> { "a.IsActive = true" };
                var parameters = new DynamicParameters(new { Take = take });

                ApplyDetailFilters(filter, "a.Date", whereClause, parameters);

                sqlQuery += " WHERE " + string.Join(" AND ", whereClause);
                sqlQuery += " ORDER BY a.Date DESC LIMIT @Take";

                var data = await _db.QueryAsync<AppointmentsDetailResponse>(sqlQuery, parameters);

                var result = new DataResultDTO<AppointmentsDetailResponse>
                {
                    Data = data,
                    TotalData = data.Count()
                };
                return result;
            }
        }

        public async Task<IEnumerable<AppointmentsDetailResponse>> GetAllDetailListToday(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var currentDate = DateTime.Now.Date;
                var sqlQuery = BuildAppointmentDetailSelect(includeActivityInfo: false);
                var parameters = new DynamicParameters(new
                {
                    CurrentDate = currentDate,
                    NextDate = currentDate.AddDays(1)
                });

                sqlQuery += " WHERE a.IsActive = true AND a.Date >= @CurrentDate AND a.Date < @NextDate";

                return await _db.QueryAsync<AppointmentsDetailResponse>(sqlQuery, parameters);
            }
        }

        public async Task<AppointmentsDetailResponse> GetAllDetail(int id, string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var sqlQuery = BuildAppointmentDetailSelect(includeActivityInfo: true);
                sqlQuery += " WHERE a.Id = @Id AND a.IsActive = true";

                return await _db.QueryFirstAsync<AppointmentsDetailResponse>(sqlQuery, new { Id = id });
            }
        }

        public async Task<IEnumerable<AppointmentsStatus>> GetAllStatus(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                return await _db.QueryAsync<AppointmentsStatus>($"SELECT * FROM AppointmentsStatus Where IsActive = true");
            }
        }

        public async Task<int> AddActivity(AppointmentsActivity entity, string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {

                var propertyNames = QueryGenerator.GetPropertyNames(entity);

                var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
                var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

                var query = $"INSERT INTO AppointmentsActivity ({columnNames}) VALUES ({parameterNames}); SELECT LAST_INSERT_ID();";
                return await _db.ExecuteScalarAsync<int>(query, entity);
            }
        }

        public async Task<IEnumerable<Appointments>> GetAllByStatusId(string dbName, int statusId)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                return await _db.QueryAsync<Appointments>($"SELECT * FROM Appointments WHERE StatusId = @Id AND IsActive = true", new { Id = statusId });
            }
        }

        public async Task<DataResultDTO<AppointmentMedicalDetailResponse>> GetAllDetailMedicalList(string dbName, AppointmentDetailFilter filter)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var sqlQuery = BuildAppointmentDetailSelect(includeActivityInfo: false, medicalOnly: true);
                var whereClause = new List<string> { "a.IsActive = true" };
                var parameters = new DynamicParameters();

                ApplyDetailFilters(filter, "a.Date", whereClause, parameters);

                sqlQuery += " WHERE " + string.Join(" AND ", whereClause);

                var data = await _db.QueryAsync<AppointmentMedicalDetailResponse>(sqlQuery, parameters);

                var result = new DataResultDTO<AppointmentMedicalDetailResponse>
                {
                    Data = data,
                    TotalData = data.Count()
                };
                return result;
            }
        }

        public async Task<IEnumerable<DataPoint>> GetClientWeek(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                // Modify your query to filter by the current week's start and end
                string query = $@"
                        SELECT 
                            DAYOFWEEK(Date) AS W,
                            HOUR(Date) AS X,
                            FLOOR(AVG(COUNT(Id)) OVER (PARTITION BY DAYOFWEEK(Date), HOUR(DATE))) AS Y
                        FROM 
                            Appointments
                        WHERE 
                            StatusId IN (2, 3, 4, 5, 6) AND IsActive = true
                        GROUP BY 
                            DAYOFWEEK(Date), HOUR(Date)
                        ORDER BY 
                            W, X;
            ";

                var result = await _db.QueryAsync<DataPoint>(query);
                return result;
            }
        }

        public async Task<IEnumerable<AppointmentsDetailReport>> GetDetailReport(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var query = $@"SELECT 
                       a.Id AS AppointmentId, 
                       COALESCE(mr.Id, 0) AS MedicalRecordId,
                       a.OwnersId, 
                       mr.Code, 
                       o.Name AS OwnersName, 
                       o.Title AS OwnersTitle, 
                       a.PatientsId, 
                       p.Name AS PatientsName, 
                       p.Breed AS PatientsBreed, 
                       a.ServiceId, 
                       COALESCE(s.Name, a.Type) AS ServiceName,
                       at.Color AS TypeColor,
                       a.StaffId, 
                       pr.Name AS StaffName, 
                       a.StatusId, 
                       st.Name AS StatusName, 
                       a.Notes, 
                       a.Date, 
                       s.Duration AS DurationEstimate, 
                       s.DurationType AS DurationTypeEstimate, 
                       CASE
                           WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE)
                           WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR)
                           WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY)
                           WHEN s.DurationType IS NULL THEN a.Date
                           ELSE NULL
                       END AS EndDateEstimate,
                       s.Price AS Total,
                       CASE
                           WHEN op.MedicalRecordId IS NOT NULL THEN TRUE
                           ELSE FALSE
                       END AS IsOpname,
                       CASE
                           WHEN a.Type IS NOT NULL THEN a.Type
                           WHEN a.ServiceId IS NOT NULL AND a.Type IS NULL THEN s.Name
                           ELSE NULL
                       END AS Type,
                       CASE
                           WHEN acts.MedicalRecordCount > 1 THEN TRUE
                           ELSE FALSE
                       END AS IsEdit,
                       act.InvoiceDate, 

                       mr.DiscountValue,
                       mr.DiscountTotal,
                       mr.TotalDiscounted,

                       pm.Name AS PaymentMethod   -- 🔥 ambil payment method terakhir
                FROM Appointments a
                JOIN Owners o ON o.Id = a.OwnersId
                JOIN Patients p ON p.Id = a.PatientsId
                LEFT JOIN Services s ON s.Id = a.ServiceId
                LEFT JOIN AppointmentsType at ON at.Name = a.Type
                JOIN Profile pr ON pr.Id = a.StaffId
                JOIN AppointmentsStatus st ON st.Id = a.StatusId
                LEFT JOIN MedicalRecords mr ON mr.AppointmentId = a.Id
                LEFT JOIN (
                    SELECT DISTINCT MedicalRecordId
                    FROM OpnamePatients
                ) op ON op.MedicalRecordId = mr.Id
                -- Join dengan AppointmentsActivity buat dapatkan status count
                LEFT JOIN (
                    SELECT AppointmentId, COUNT(*) AS MedicalRecordCount
                    FROM AppointmentsActivity
                    WHERE CurrentStatusId = 3
                    GROUP BY AppointmentId
                ) acts ON acts.AppointmentId = a.Id
                -- Join dengan AppointmentsActivity buat dapatkan invoice date terakhir
                LEFT JOIN (
                    SELECT AppointmentId, MAX(CurrentDate) AS InvoiceDate
                    FROM AppointmentsActivity
                    WHERE CurrentStatusId = 6
                    GROUP BY AppointmentId
                ) act ON act.AppointmentId = a.Id
                -- Join dengan payment terakhir untuk medical record
                LEFT JOIN (
                    SELECT op.OrderId, op.Type, op.PaymentMethodId
                    FROM OrdersPayment op
                    INNER JOIN (
                        SELECT OrderId, Type, MAX(Date) AS MaxDate
                        FROM OrdersPayment
                        WHERE IsActive = 1 AND Type = 'MedicalRecord'
                        GROUP BY OrderId, Type
                    ) last_op
                        ON op.OrderId = last_op.OrderId
                       AND op.Type = last_op.Type
                       AND op.Date = last_op.MaxDate
                ) lastpay ON lastpay.OrderId = mr.Id AND lastpay.Type = 'MedicalRecord'
                LEFT JOIN PaymentMethod pm ON lastpay.PaymentMethodId = pm.Id
                WHERE a.IsActive = true;
                ";
                return await _db.QueryAsync<AppointmentsDetailReport>(query);
            }
        }
        public async Task<IEnumerable<string>> GetDetailReportFilter(string dbName, string filterField)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                // Mapping field agar aman (hindari SQL Injection)
                var allowedFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Code", "mr.Code" },
                { "OwnersName", "o.Name" },
                { "PatientsName", "p.Name" },
                { "StaffName", "pr.Name" },
                { "ServiceName", "s.Name" },
                { "Date", "a.Date" },
                { "Total", "s.Price" },
                { "Type", "a.Type" },
                { "StatusName", "st.Name" },
                { "DiscountValue", "mr.DiscountValue" },
                { "DiscountTotal", "mr.DiscountTotal" },
                { "TotalDiscounted", "mr.TotalDiscounted" },
                { "PaymentMethod", "pm.Name" } // 🔥 tambahin filter PaymentMethod
            };

                if (!allowedFields.ContainsKey(filterField))
                    throw new ArgumentException("Invalid filter field");

                var column = allowedFields[filterField];

                var sql = $@"
                    SELECT DISTINCT {column}
                    FROM Appointments a
                    JOIN Owners o ON o.Id = a.OwnersId
                    JOIN Patients p ON p.Id = a.PatientsId
                    LEFT JOIN Services s ON s.Id = a.ServiceId
                    JOIN Profile pr ON pr.Id = a.StaffId
                    JOIN AppointmentsStatus st ON st.Id = a.StatusId
                    LEFT JOIN MedicalRecords mr ON mr.AppointmentId = a.Id
                    LEFT JOIN (
                        SELECT DISTINCT MedicalRecordId
                        FROM OpnamePatients
                    ) op ON op.MedicalRecordId = mr.Id
                    LEFT JOIN (
                        SELECT AppointmentId, COUNT(*) AS MedicalRecordCount
                        FROM AppointmentsActivity
                        WHERE CurrentStatusId = 3
                        GROUP BY AppointmentId
                    ) acts ON acts.AppointmentId = a.Id
                    LEFT JOIN (
                        SELECT AppointmentId, MAX(CurrentDate) AS InvoiceDate
                        FROM AppointmentsActivity
                        WHERE CurrentStatusId = 6
                        GROUP BY AppointmentId
                    ) act ON act.AppointmentId = a.Id
                    -- join last payment untuk medical record
                    LEFT JOIN (
                        SELECT op.OrderId, op.Type, op.PaymentMethodId
                        FROM OrdersPayment op
                        INNER JOIN (
                            SELECT OrderId, Type, MAX(Date) AS MaxDate
                            FROM OrdersPayment
                            WHERE IsActive = 1 AND Type = 'MedicalRecord'
                            GROUP BY OrderId, Type
                        ) last_op
                            ON op.OrderId = last_op.OrderId
                           AND op.Type = last_op.Type
                           AND op.Date = last_op.MaxDate
                    ) lastpay ON lastpay.OrderId = mr.Id AND lastpay.Type = 'MedicalRecord'
                    LEFT JOIN PaymentMethod pm ON lastpay.PaymentMethodId = pm.Id
                    WHERE a.IsActive = true
                      AND {column} IS NOT NULL
                    ORDER BY {column};
                ";
                return await _db.QueryAsync<string>(sql);
            }
        }
    }
}
